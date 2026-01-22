using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A group of matches sharing similar settings
/// </summary>
public partial class MatchGroupViewModel : ViewModelBase, IReadOnlyList<MatchViewModel>
{
    #region Properties

    /// <summary>
    /// The settings for this group
    /// </summary>
    [ObservableProperty]
    public partial MatchSettingsViewModel Settings { get; set; } = new();

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

    public MatchViewModel this[int index] => index >= 0 && index < Matches.Count ? Matches[index] : new MatchNotFoundViewModel();

    public int Count => Matches.Count;

    public IEnumerator<MatchViewModel> GetEnumerator() => Matches.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    /// <summary>
    /// Gets the record representation of this view model
    /// </summary>
    public MatchGroup ToModel() => new()
    {
        Matches = [.. Matches.Select(m => m.Guid)],
        Settings = Settings.ToModel()
    };

    /// <summary>
    /// Loads a match group from the given model
    /// </summary>
    public static MatchGroupViewModel FromModel(MatchGroup model)
    {
        MatchGroupViewModel vm = new()
        {
            Settings = MatchSettingsViewModel.FromModel(model.Settings)
        };

        // load matches
        vm.Matches = [.. model.Matches.Select(id =>
        {
            var newmatch = MatchViewModel.FromModel(StorageService.GetMatch(id));
            newmatch.Settings = vm.Settings;
            vm.AddMatchListeners(newmatch);
            return newmatch;
        })];

        return vm;
    }

    /// <summary>
    /// Adds a new match to the group, setting the <see cref="MatchViewModel.Settings"/> and orignal number
    /// and returns it
    /// </summary>
    public T NewMatch<T>(ParticipantViewModel first, ParticipantViewModel second, int? number = null) where T : MatchViewModel, new() =>
        Add(new T()
        {
            Settings = Settings,
            Number = number,
            First = new ScoreViewModel(first),
            Second = new ScoreViewModel(second)
        });

    /// <summary>
    /// Adds a new match to the group, setting the <see cref="MatchViewModel.Settings"/> and orignal number
    /// and returns it
    /// </summary>
    public T NewMatch<T>(ParticipantViewModel? first, int firstSeed, ParticipantViewModel? second, int secondSeed) where T : MatchViewModel, new() =>
        Add(new T()
        {
            Settings = Settings,
            First = first is not null ? new ScoreViewModel(first) { Seed = firstSeed } : new ScoreViewModel(ParticipantViewModel.Bye),
            Second = second is not null ? new ScoreViewModel(second) { Seed = secondSeed } : new ScoreViewModel(ParticipantViewModel.Bye)
        });

    /// <summary>
    /// Adds a new match to the group that will be bound to the winners of the given matches
    /// </summary>
    public T NewMatchFromWinnersOf<T>(MatchViewModel parent1, MatchViewModel parent2) where T : MatchViewModel, new() =>
        Add(new T()
        {
            Settings = Settings,
            First = ScoreViewModel.WinnerOf(parent1),
            Second = ScoreViewModel.WinnerOf(parent2)
        });

    /// <summary>
    /// Adds a new match to the group that will be bound to the losers of the given matches
    /// </summary>
    public T NewMatchFromLosersOf<T>(MatchViewModel parent1, MatchViewModel parent2) where T : MatchViewModel, new() =>
        Add(new T()
        {
            Settings = Settings,
            First = ScoreViewModel.LoserOf(parent1),
            Second = ScoreViewModel.LoserOf(parent2)
        });

    /// <summary>
    /// Adds the given match
    /// </summary>
    private T Add<T>(T match) where T : MatchViewModel
    {
        Matches.Add(match);
        AddMatchListeners(match);
        return match;
    }

    /// <summary>
    /// Permanently removes all matches from storage and clears the group
    /// </summary>
    public void PermanentlyDeleteAll()
    {
        foreach (var match in Matches)
            StorageService.Delete<Match>(match.Guid);
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
    public void Save() => StorageService.WriteMatches(Matches
        .Select(m => m.ToModel())
        );

    /// <summary>
    /// Determines if the given participant is in any of the matches in this group
    /// </summary>
    public bool Contains(ParticipantViewModel participant) => Matches.Any(m => m.Contains(participant));
}
