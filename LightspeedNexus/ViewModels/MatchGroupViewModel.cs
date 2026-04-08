using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Network;
using Lightspeed.Network.Messages;
using Lightspeed.Services;
using Lightspeed.ViewModels;
using LightspeedNexus.Services;
using System.Collections;
using System.Collections.ObjectModel;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A group of matches sharing similar settings
/// </summary>
public partial class MatchGroupViewModel : ViewModelBase, IReadOnlyList<MatchViewModel>
{
    private readonly StorageService _storageService;
    private readonly MatchFactory _matchFactory;

    #region Properties

    [ObservableProperty]
    public partial Guid Guid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The settings for this group
    /// </summary>
    [ObservableProperty]
    public partial MatchSettingsViewModel Settings { get; set; }

    /// <summary>
    /// The name of this group of matches
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = "";

    /// <summary>
    /// Set by the parent when it no longer allows changes to the settings
    /// </summary>
    public bool IsStarted => Matches.Any(m => m.IsMatchStarted);

    /// <summary>
    /// Determines if all the matches have finished
    /// </summary>
    public bool IsCompleted => Matches.All(m => m.IsMatchCompleted);

    /// <summary>
    /// The matches that belong to this group
    /// </summary>
    public ObservableCollection<MatchViewModel> Matches { get; private set; } = [];

    /// <summary>
    /// Determines if this group has no matches
    /// </summary>
    public bool IsEmpty => Matches.Count == 0;

    #endregion

    #region List implementation

    public MatchViewModel this[int index] => index >= 0 && index < Matches.Count ? Matches[index] : New<MatchNotFoundViewModel>();

    public int Count => Matches.Count;

    public IEnumerator<MatchViewModel> GetEnumerator() => Matches.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    public MatchGroupViewModel(IServiceProvider serviceProvider, IMessenger messenger, SquadronService squadronService, StorageService storageService, MatchFactory matchFactory)
        : base(serviceProvider, messenger)
    {
        _storageService = storageService;
        _matchFactory = matchFactory;

        Settings = New<MatchSettingsViewModel>();

        // listen to network requests for match summaries
        messenger.Register<RequestMatchGroupSummaries, Guid>(this, Guid,
            (r, m) =>
            {
                var summaries = new MatchSummaries
                {
                    Summaries = [.. Matches.Select(m =>
                    {
                        var summary = m.ToSummary();

                        try
                        {
                            summary.Color = squadronService.FindSquadronColor(Name);
                        }
                        catch { }

                        return summary;
                    })]
                };

                m.Reply(summaries);
            }
        );

        // listen to network requests for a given match's state and the next match
        messenger.Register<RequestNextMatch, Guid>(this, Guid,
            (r, m) =>
            {
                MatchViewModel? match = null;
                string? next = null;

                // find the match we want
                int i = 0;
                for (; i < Matches.Count && match is null; i++)
                {
                    if (Matches[i].Guid == m.MatchId)
                        match = Matches[i];
                }

                // find the next incomplete match
                for (; i < Matches.Count && match is not null && string.IsNullOrEmpty(next); i++)
                {
                    if (!Matches[i].IsMatchCompleted)
                        next = $"Next: {Matches[i].First?.Participant.Name} v. {Matches[i].Second?.Participant.Name}";
                }

                m.Reply(next);
            }
        );
    }

    /// <summary>
    /// Gets the record representation of this view model
    /// </summary>
    public MatchGroup ToModel() => new()
    {
        Id = Guid,
        Matches = [.. Matches.Select(m => m.Guid)],
        Settings = Settings.ToModel()
    };

    /// <summary>
    /// Adds the given match
    /// </summary>
    public T Add<T>(T match) where T : MatchViewModel
    {
        Matches.Add(match);
        AddMatchListeners(match);
        return match;
    }

    /// <summary>
    /// Adds a new match to the group, setting the <see cref="MatchViewModel.Settings"/> and orignal number
    /// and returns it
    /// </summary>
    public T NewMatch<T>(ParticipantViewModel first, ParticipantViewModel second, int? number = null) where T : MatchViewModel
    {
        var match = _matchFactory.NewMatch<T>(first, second, number);
        match.Settings = Settings;
        return Add(match);
    }

    /// <summary>
    /// Adds a new match to the group, setting the <see cref="MatchViewModel.Settings"/> and orignal number
    /// and returns it
    /// </summary>
    public T NewMatch<T>(ParticipantViewModel? first, int firstSeed, ParticipantViewModel? second, int secondSeed) where T : MatchViewModel
    {
        var match = _matchFactory.NewMatch<T>(first, firstSeed, second, secondSeed);
        match.Settings = Settings;
        return Add(match);
    }

    /// <summary>
    /// Adds a new match to the group that will be bound to the winners of the given matches
    /// </summary>
    public T NewEmptyMatch<T>() where T : MatchViewModel
    {
        var match = _matchFactory.NewEmptyMatch<T>();
        match.Settings = Settings;
        return Add(match);
    }

    /// <summary>
    /// Permanently removes all matches from storage and clears the group
    /// </summary>
    public void PermanentlyDeleteAll()
    {
        foreach (var match in Matches)
            _storageService.Delete<Match>(match.Guid);
        Matches.Clear();
    }

    /// <summary>
    /// Adds listeners to the given match
    /// </summary>
    private void AddMatchListeners(MatchViewModel match) => match.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(MatchViewModel.IsMatchStarted))
        {
            OnPropertyChanged(nameof(IsStarted));
            Settings.IsLocked = IsStarted;
        }
        else if (e.PropertyName == nameof(MatchViewModel.IsMatchCompleted))
            OnPropertyChanged(nameof(IsCompleted));
    };

    /// <summary>
    /// Get the match with the given id
    /// </summary>
    public MatchViewModel? GetMatch(Guid id) => Matches.FirstOrDefault(m => m.Guid == id);

    /// <summary>
    /// Saves all the matches
    /// </summary>
    public void Save() => _storageService.WriteMatches(Matches
        .Select(m => m.ToModel())
        );

    /// <summary>
    /// Determines if the given participant is in any of the matches in this group
    /// </summary>
    public bool Contains(ParticipantViewModel participant) => Matches.Any(m => m.Contains(participant));
}
