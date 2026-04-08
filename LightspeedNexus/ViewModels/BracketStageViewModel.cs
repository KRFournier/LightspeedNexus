using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Network;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using LightspeedNexus.Transitions;

namespace LightspeedNexus.ViewModels;

#region Messages

public sealed class BracketRoundCompleted() { };

#endregion

public partial class BracketStageViewModel : StageViewModel, IRecipient<MatchWinnerChangedMessage>
{
    #region Properties

    public override string Name => "Bracket";

    /// <summary>
    /// The top 64 matches
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Top32Title))]
    [NotifyPropertyChangedFor(nameof(Top16Title))]
    public partial MatchGroupViewModel Top64 { get; set; }
    partial void OnTop64Changed(MatchGroupViewModel value) => SetGroupListeners(value);

    /// <summary>
    /// The top 32 matches
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Top32Title))]
    [NotifyPropertyChangedFor(nameof(Top16Title))]
    public partial MatchGroupViewModel Top32 { get; set; }
    public string? Top32Title => Top64.Matches.Count > 0 ? "Round 2" : "Round 1";
    partial void OnTop32Changed(MatchGroupViewModel value) => SetGroupListeners(value);

    /// <summary>
    /// The top 16 matches
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Top16Title))]
    public partial MatchGroupViewModel Top16 { get; set; }
    public string? Top16Title => Top64.Matches.Count > 0 ? "Round 3" : (Top32.Matches.Count > 0 ? "Round 2" : "Round 1");
    partial void OnTop16Changed(MatchGroupViewModel value) => SetGroupListeners(value);

    /// <summary>
    /// The top 8 matches
    /// </summary>
    [ObservableProperty]
    public partial MatchGroupViewModel Quarterfinals { get; set; }
    partial void OnQuarterfinalsChanged(MatchGroupViewModel value) => SetGroupListeners(value);

    /// <summary>
    /// The top 4 matches
    /// </summary>
    [ObservableProperty]
    public partial MatchGroupViewModel Semifinals { get; set; }
    partial void OnSemifinalsChanged(MatchGroupViewModel value) => SetGroupListeners(value);

    /// <summary>
    /// The optional third place match
    /// </summary>
    [ObservableProperty]
    public partial MatchGroupViewModel Third { get; set; }
    partial void OnThirdChanged(MatchGroupViewModel value) => SetGroupListeners(value);

    /// <summary>
    /// The final match
    /// </summary>
    [ObservableProperty]
    public partial MatchGroupViewModel Final { get; set; }
    partial void OnFinalChanged(MatchGroupViewModel value) => SetGroupListeners(value);

    /// <summary>
    /// Determines if at least one match has started
    /// </summary>
    public bool IsStarted => EnumerateGroups().Any(g => g.IsStarted);

    /// <summary>
    /// Determines if the brackets have finished
    /// </summary>
    public bool IsCompleted => Final.Matches[0].IsMatchCompleted;

    /// <summary>
    /// Counts all the matches
    /// </summary>
    public int MatchCount => EnumerateGroups().Sum(g => g.Matches.Count);

    /// <summary>
    /// How many wins can we have
    /// </summary>
    public int PossibleWins
    {
        get
        {
            if (Top64 is not null)
                return 6;
            else if (Top32 is not null)
                return 5;
            else if (Top16 is not null)
                return 4;
            else if (Quarterfinals is not null)
                return 3;
            else
                return 2;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTop16Ranked))]
    [NotifyPropertyChangedFor(nameof(IsTop8Ranked))]
    [NotifyPropertyChangedFor(nameof(IsTop4Ranked))]
    [NotifyPropertyChangedFor(nameof(IsTop2Ranked))]
    public partial bool IsRanked { get; set; }

    private readonly int _topXForFinals;
    public bool IsTop16Ranked => IsRanked && _topXForFinals == 16;
    public bool IsTop8Ranked => IsRanked && _topXForFinals == 8;
    public bool IsTop4Ranked => IsRanked && _topXForFinals == 4;
    public bool IsTop2Ranked => IsRanked && _topXForFinals == 2;

    #endregion

    #region Commands

    protected override bool CanGoNext() => IsCompleted;

    #endregion

    #region Message Handlers

    /// <summary>
    /// Called when a match winner is changed. 
    /// </summary>
    public void Receive(MatchWinnerChangedMessage message)
    {
        var match = message.Value;

        // find this match's group
        MatchGroupViewModel? group = EnumerateGroups().FirstOrDefault(g => g.Matches.Contains(match));

        // if this is the final or third place match, we don't need to advance anyone
        if (group is null || group == Final || group == Third)
            return;

        // find the match's index into the group
        int index = group.Matches.IndexOf(match);

        // calculate the index of the next match this feeds into.
        int nextIndex = index / 2;

        // get the next group.
        var nextGroup = FindBracketGroup(group.Matches.Count);

        // advance the winner. If index is even, winner goes to first slot, else second slot
        if (index % 2 == 0)
            nextGroup[nextIndex].First.Participant = match.Winner?.Participant ?? New<EmptyParticipantViewModel>();
        else
            nextGroup[nextIndex].Second.Participant = match.Winner?.Participant ?? New<EmptyParticipantViewModel>();

        // advance the loser to the third place match if this is the semifinals
        if (group == Semifinals)
        {
            if (index % 2 == 0)
                Third[0].First.Participant = match.Loser?.Participant ?? New<EmptyParticipantViewModel>();
            else
                Third[0].Second.Participant = match.Loser?.Participant ?? New<EmptyParticipantViewModel>();
        }
    }

    #endregion

    /// <summary>
    /// Creates a new view model with default settings, populating test data when in design mode
    /// </summary>
    public BracketStageViewModel(IServiceProvider serviceProvider, IMessenger messenger, NavigationService navigationService, ActiveTournamentService activeTournamentService)
        : base(serviceProvider, messenger, navigationService)
    {
        Top64 = NewBracketMatchGroup("Top 64", 16);
        Top32 = NewBracketMatchGroup("Top 32", 16);
        Top16 = NewBracketMatchGroup("Top 16", 16);
        Quarterfinals = NewBracketMatchGroup("Quarterfinals", 16);
        Semifinals = NewBracketMatchGroup("Semifinals", 16);
        Third = NewBracketMatchGroup("Third Place", 24);
        Final = NewBracketMatchGroup("Final", 32);

        // listen for match winner changes to advance players
        messenger.Register<MatchWinnerChangedMessage>(this);

        if (Design.IsDesignMode)
        {
            //_topXForFinals = GradingsChart.GetTopX(50);
            //var rankings = Enumerable.Range(1, 64).Select(i =>
            //{
            //    if (i >= 50)
            //        return ParticipantViewModel.Bye;
            //    return new RegistreeViewModel() { FirstName = "Player", LastName = $"{i}" }.ToPlayer();
            //});
            //int playerCount = rankings.Count();
            //int bracketCount = FindBracketCount(playerCount, true);
            //var list = BuildList(rankings, bracketCount);
            //var group = SetInitialRoundTo(bracketCount, list);
            //while (group.Matches.Count > 2)
            //    group = SetNextRoundFor(group);
            //Final.NewMatchFromWinnersOf<StandardMatchViewModel>(group.Matches[0], group.Matches[1]);
            //Third.NewMatchFromLosersOf<StandardMatchViewModel>(group.Matches[0], group.Matches[1]);
        }
        else
        {
            _topXForFinals = GradingsChart.GetTopX(activeTournamentService.RegistreeCount);
        }
    }

    /// <summary>
    /// Loads a match group from the given model
    /// </summary>
    protected MatchGroupViewModel NewBracketMatchGroup(string name, int winningScore)
    {
        var vm = New<MatchGroupViewModel>();
        vm.Name = name;
        vm.Settings = New<MatchSettingsViewModel>();
        vm.Settings.WinningScore = winningScore;
        vm.Settings.TimeLimit = TimeSpan.FromSeconds(winningScore / 4 * 30);
        vm.Settings.Rounds = 2;
        return vm;
    }

    /// <summary>
    /// Brackets always go to results
    /// </summary>
    public override IStageTransition GetTransitionToNextStage() => NewTransition<BracketToResultsTransition>();

    /// <summary>
    /// Exports this view model to a <see cref="BracketStage"/>.
    /// </summary>
    public override BracketStage ToModel() => new()
    {
        Top64 = Top64.ToModel(),
        Top32 = Top32.ToModel(),
        Top16 = Top16.ToModel(),
        Quarterfinals = Quarterfinals.ToModel(),
        Semifinals = Semifinals.ToModel(),
        Third = Third.ToModel(),
        Final = Final.ToModel(),
        Next = Next?.ToModel()
    };

    /// <summary>
    /// Gets the state of each match group.
    /// </summary>
    public IEnumerable<MatchGroupState> GetMatchGroupsStates()
    {
        if (Top64.Matches.Count > 0)
            yield return new MatchGroupState() { Name = "Top 64", IsCompleted = Top64.IsCompleted };
        if (Top32.Matches.Count > 0)
            yield return new MatchGroupState() { Name = "Top 32", IsCompleted = Top32.IsCompleted };
        if (Top16.Matches.Count > 0)
            yield return new MatchGroupState() { Name = "Top 16", IsCompleted = Top16.IsCompleted };
        if (Quarterfinals.Matches.Count > 0)
            yield return new MatchGroupState() { Name = "Quarterfinals", IsCompleted = Quarterfinals.IsCompleted };
        if (Semifinals.Matches.Count > 0)
            yield return new MatchGroupState() { Name = "Semifinals", IsCompleted = Semifinals.IsCompleted };
        yield return new MatchGroupState() { Name = "Third", IsCompleted = Third.IsCompleted };
        yield return new MatchGroupState() { Name = "Final", IsCompleted = Final.IsCompleted };
    }

    /// <summary>
    /// Sets listeners for a specific group. This is necessary to update the stage's IsStarted and IsCompleted properties,
    /// </summary>
    /// <param name="group"></param>
    protected void SetGroupListeners(MatchGroupViewModel group)
    {
        group.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MatchGroupViewModel.IsStarted))
                OnPropertyChanged(nameof(IsStarted));
            else if (e.PropertyName == nameof(MatchGroupViewModel.IsCompleted))
            {
                OnPropertyChanged(nameof(IsCompleted));
                StrongReferenceMessenger.Default.Send(new BracketRoundCompleted());
                GoNextCommand.NotifyCanExecuteChanged();
            }
        };
        OnPropertyChanged(nameof(IsStarted));
        OnPropertyChanged(nameof(IsCompleted));
        GoNextCommand.NotifyCanExecuteChanged();
    }

    #region Match Enumeration and Lookup

    /// <summary>
    /// Enumerates through all the match groups. Some groups might be empty because they
    /// are not in use, but they are never null.
    /// </summary>
    public IEnumerable<MatchGroupViewModel> EnumerateGroups()
    {
        if (Top64.Matches.Count > 0)
            yield return Top64;
        if (Top32.Matches.Count > 0)
            yield return Top32;
        if (Top16.Matches.Count > 0)
            yield return Top16;
        if (Quarterfinals.Matches.Count > 0)
            yield return Quarterfinals;
        if (Semifinals.Matches.Count > 0)
            yield return Semifinals;
        yield return Third;
        yield return Final;
    }


    /// <summary>
    /// Enumerates through all the bracket matches from the earliest to the latest
    /// </summary>
    public IEnumerable<MatchViewModel> EnumerateMatches()
    {
        foreach (var match in Top64)
            yield return match;
        foreach (var match in Top32)
            yield return match;
        foreach (var match in Top16)
            yield return match;
        foreach (var match in Quarterfinals)
            yield return match;
        foreach (var match in Semifinals)
            yield return match;
        yield return Third[0];
        yield return Final[0];
    }

    /// <summary>
    /// Finds the group based on the exact number of players in that group
    /// </summary>
    public MatchGroupViewModel FindBracketGroup(int numPlayers) => numPlayers switch
    {
        64 => Top64,
        32 => Top32,
        16 => Top16,
        8 => Quarterfinals,
        4 => Semifinals,
        2 => Final,
        _ => throw new ArgumentOutOfRangeException(nameof(numPlayers), "Parameter must be 2, 4, 8, 16, 32, or 64")
    };

    /// <summary>
    /// Returns the players for the given tier.
    /// </summary>
    /// <param name="x">The top X players. Must be a power of 2.</param>
    /// <returns>Each player in no particular order</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if x does not reference a valid tier</exception>
    public IEnumerable<ParticipantViewModel> GetTopXParticipants(int x)
    {
        MatchGroupViewModel group = FindBracketGroup(x);
        foreach (var match in group)
        {
            yield return match.First.Participant;
            yield return match.Second.Participant;
        }
    }

    #endregion

    public override void OnTournamentSaved()
    {
        foreach (var group in EnumerateGroups())
            group.Save();
        Next?.OnTournamentSaved();
    }

    protected override void OnGoingBack()
    {
        foreach (var group in EnumerateGroups())
            group.PermanentlyDeleteAll();
    }
}
