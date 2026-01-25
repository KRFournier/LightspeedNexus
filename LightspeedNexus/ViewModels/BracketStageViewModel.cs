using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace LightspeedNexus.ViewModels;

#region Messages

public sealed class RequestBracketMatch(Guid id) : RequestMessage<MatchViewModel>
{
    public Guid Id { get; } = id;
}

public sealed class BracketRoundCompleted() { };

#endregion

public partial class BracketStageViewModel : StageViewModel, IRecipient<RequestBracketMatch>
{
    #region Properties

    /// <summary>
    /// The top 64 matches
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Top32Title))]
    [NotifyPropertyChangedFor(nameof(Top16Title))]
    public partial MatchGroupViewModel Top64 { get; set; } = new() { Settings = new() { WinningScore = 16, TimeLimit = TimeSpan.FromMinutes(2), Rounds = 2 } };

    /// <summary>
    /// The top 32 matches
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Top32Title))]
    [NotifyPropertyChangedFor(nameof(Top16Title))]
    public partial MatchGroupViewModel Top32 { get; set; } = new() { Settings = new() { WinningScore = 16, TimeLimit = TimeSpan.FromMinutes(2), Rounds = 2 } };
    public string? Top32Title => Top64.Matches.Count > 0 ? "Round 2" : "Round 1";

    /// <summary>
    /// The top 16 matches
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Top16Title))]
    public partial MatchGroupViewModel Top16 { get; set; } = new() { Settings = new() { WinningScore = 16, TimeLimit = TimeSpan.FromMinutes(2), Rounds = 2 } };
    public string? Top16Title => Top64.Matches.Count > 0 ? "Round 3" : (Top32.Matches.Count > 0 ? "Round 2" : "Round 1");

    /// <summary>
    /// The top 8 matches
    /// </summary>
    [ObservableProperty]
    public partial MatchGroupViewModel Quarterfinals { get; set; } = new() { Settings = new() { WinningScore = 16, TimeLimit = TimeSpan.FromMinutes(2), Rounds = 2 } };

    /// <summary>
    /// The top 4 matches
    /// </summary>
    [ObservableProperty]
    public partial MatchGroupViewModel Semifinals { get; set; } = new() { Settings = new() { WinningScore = 16, TimeLimit = TimeSpan.FromMinutes(2), Rounds = 2 } };

    /// <summary>
    /// The optional third place match
    /// </summary>
    [ObservableProperty]
    public partial MatchGroupViewModel Third { get; set; } = new() { Settings = new() { WinningScore = 24, TimeLimit = TimeSpan.FromMinutes(3), Rounds = 2 } };

    /// <summary>
    /// The final match
    /// </summary>
    [ObservableProperty]
    public partial MatchGroupViewModel Final { get; set; } = new() { Settings = new() { WinningScore = 32, TimeLimit = TimeSpan.FromMinutes(4), Rounds = 2 } };

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

    /// <summary>
    /// Go to the Final Results Stage
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoToResults))]
    private void GoToResults() => Next = ResultsStageViewModel.FromBracket(this);
    public bool CanGoToResults() => IsCompleted;

    #endregion

    #region Message Handlers

    public void Receive(RequestBracketMatch message)
    {
        foreach(var group in EnumerateGroups())
        {
            var match = group.Matches.FirstOrDefault(m => m.Guid == message.Id);
            if (match is not null)
            {
                message.Reply(match);
                return;
            }
        }
    }

    #endregion

    /// <summary>
    /// Creates a new view model with default settings, populating test data when in design mode
    /// </summary>
    public BracketStageViewModel() : base("Bracket")
    {
        if (Design.IsDesignMode)
        {
            _topXForFinals = GradingsChart.GetTopX(50);
            var rankings = Enumerable.Range(1, 64).Select(i =>
            {
                if (i >= 50)
                    return ParticipantViewModel.Bye;
                return PlayerViewModel.FromRegistree(new RegistreeViewModel() { FirstName = "Player", LastName = $"{i}" });
            });
            int playerCount = rankings.Count();
            int bracketCount = FindBracketCount(playerCount, true);
            var list = BuildList(rankings, bracketCount);
            var group = SetInitialRoundTo(bracketCount, list);
            while (group.Matches.Count > 2)
                group = SetNextRoundFor(group);
            Final.NewMatchFromWinnersOf<StandardMatchViewModel>(group.Matches[0], group.Matches[1]);
            Third.NewMatchFromLosersOf<StandardMatchViewModel>(group.Matches[0], group.Matches[1]);
        }
        else
        {
            _topXForFinals = GradingsChart.GetTopX(StrongReferenceMessenger.Default.Send(new RequestRegistreeCount()));
            StrongReferenceMessenger.Default.RegisterAll(this);
            SetGroupListeners();
        }
    }

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
    /// Loads from a <see cref="BracketStage"/>
    /// </summary>
    public static BracketStageViewModel FromModel(BracketStage model)
    {
        BracketStageViewModel vm = new()
        {
            Top64 = MatchGroupViewModel.FromModel(model.Top64),
            Top32 = MatchGroupViewModel.FromModel(model.Top32),
            Top16 = MatchGroupViewModel.FromModel(model.Top16),
            Quarterfinals = MatchGroupViewModel.FromModel(model.Quarterfinals),
            Semifinals = MatchGroupViewModel.FromModel(model.Semifinals),
            Third = MatchGroupViewModel.FromModel(model.Third),
            Final = MatchGroupViewModel.FromModel(model.Final),
            Next = FromModel(model.Next)
        };
        vm.SetGroupListeners();
        return vm;
    }

    /// <summary>
    /// Sets listeners for all the groups. This is necessary to update the stage's IsStarted and IsCompleted properties,
    /// which are used to determine if we can advance to the next stage.
    /// </summary>
    protected void SetGroupListeners()
    {
        SetGroupListeners(Top64);
        SetGroupListeners(Top32);
        SetGroupListeners(Top16);
        SetGroupListeners(Quarterfinals);
        SetGroupListeners(Semifinals);
        SetGroupListeners(Third);
        SetGroupListeners(Final);
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
                GoToResultsCommand.NotifyCanExecuteChanged();
            }
        };
        OnPropertyChanged(nameof(IsStarted));
        OnPropertyChanged(nameof(IsCompleted));
        GoToResultsCommand.NotifyCanExecuteChanged();
    }

    #region Initialization of the Bracket

    /// <summary>
    /// Predefined loadouts for optimal bracket seeding
    /// </summary>
    static readonly Dictionary<int, (int, int)[]> _loadouts = new() {
        { 2, [(0, 1)] },
        { 4, [(0, 3), (1, 2)] },
        { 8, [(0, 7), (3, 4), (2, 5), (1, 6)] },
        { 16, [(0, 15), (7, 8), (3, 12), (4, 11), (1, 14), (6, 9), (2, 13), (5, 10)] },
        { 32, [(0, 31), (15, 16), (8, 23), (7, 24), (3, 28), (12, 19), (11, 20), (4, 27), (1, 30), (14, 17), (9, 22), (6, 25), (2, 29), (13, 18), (10, 21), (5, 26)] },
        { 64, [(0, 63), (31, 32), (16, 47), (15, 48), (8, 55), (23, 40), (24, 39), (7, 56), (3, 60), (28, 35), (19, 44), (12, 51), (11, 52), (50, 43), (27, 36), (4, 59), (1, 62), (30, 33), (17, 46), (14, 49), (9, 54), (22, 41), (25, 38), (6, 57), (2, 61), (29, 34), (18, 47), (13, 50), (10, 53), (21, 42), (26, 37), (5, 58)] }
    };

    /// <summary>
    /// Creates the view model from an ordered list of participants. The list should be ordered by seed, 
    /// so the first item is the top seed, the second item is the second seed, and so on. The number of 
    /// items in the list can be less than or equal to 64. If it is less than 64, then byes will be added
    /// to fill out the bracket. The settings will determine how the bracket is filled out. For example,
    /// if 100% advancement is enabled, then the bracket will be filled out to the nearest power of 2 above
    /// the number of players. If 100% advancement is disabled, then the bracket will be filled out to
    /// the nearest power of 2 below the number of players.
    /// </summary>
    public static BracketStageViewModel FromRankedList(IEnumerable<ParticipantViewModel> rankings, bool isFullAdvancement)
    {
        var vm = new BracketStageViewModel();
        int playerCount = rankings.Count();
        int bracketCount = FindBracketCount(playerCount, isFullAdvancement);
        var list = BuildList(rankings, bracketCount);
        var group = vm.SetInitialRoundTo(bracketCount, list);
        while (group.Matches.Count > 2)
            group = vm.SetNextRoundFor(group);
        vm.Final.NewMatchFromWinnersOf<StandardMatchViewModel>(vm.Semifinals.Matches[0], vm.Semifinals.Matches[1]);
        vm.Third.NewMatchFromLosersOf<StandardMatchViewModel>(vm.Semifinals.Matches[0], vm.Semifinals.Matches[1]);
        vm.IsRanked = StrongReferenceMessenger.Default.Send<RequestIsRanked>().Response ?? false;

        return vm;
    }

    /// <summary>
    /// Given a number of players, find the bracket we want to work with. This will either
    /// round up to the nearest power of 2 (if 100% advance in enabled) or down to the
    /// nearest power of 2 (if 100% advance is disabled).
    /// </summary>
    private static int FindBracketCount(int numPlayers, bool isFullAdvancement)
    {
        if (isFullAdvancement)
            return numPlayers switch { > 32 => 64, > 16 => 32, > 8 => 16, > 4 => 8, _ => 4 };
        else
            return numPlayers switch { >= 64 => 64, >= 32 => 32, >= 16 => 16, >= 8 => 8, _ => 4 };
    }

    /// <summary>
    /// Converts rankings into a padded list of rankings to match the given count. The padded items
    /// are null. This is useful in handling byes
    /// </summary>
    private static List<ParticipantViewModel?> BuildList(IEnumerable<ParticipantViewModel> rankings, int count)
    {
        List<ParticipantViewModel?> list = [];
        foreach (var seed in rankings)
            list.Add(seed);
        while (list.Count < count)
            list.Add(null);
        return list;
    }

    /// <summary>
    /// Sets the initial round identified by the number of matches
    /// </summary>
    private MatchGroupViewModel SetInitialRoundTo(int count, List<ParticipantViewModel?> list)
    {
        var group = FindBracket(count);
        (int, int)[] loadout = _loadouts[count];
        for (int i = 0; i < loadout.Length; i++)
        {
            group.NewMatch<StandardMatchViewModel>(
                list[loadout[i].Item1], loadout[i].Item1 + 1,
                list[loadout[i].Item2], loadout[i].Item2 + 1
            );
        }
        return group;
    }

    /// <summary>
    /// Sets the next round by wiring it up to the given round's winners
    /// </summary>
    private MatchGroupViewModel SetNextRoundFor(MatchGroupViewModel previousRound)
    {
        // matches always represent two players, so if we pass the size of the previous
        // group (aka, number of matches), we'll have the correct amount of players for this
        // round, e.g. 8 matches in the previous round would be 8 players in the new one
        var group = FindBracket(previousRound.Matches.Count);
        for (int i = 0; (i + 1) < previousRound.Matches.Count; i += 2)
            group.NewMatchFromWinnersOf<StandardMatchViewModel>(previousRound.Matches[i], previousRound.Matches[i + 1]);

        return group;
    }

    #endregion

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
    private MatchGroupViewModel FindBracket(int numPlayers)
    {
        return numPlayers switch
        {
            64 => Top64,
            32 => Top32,
            16 => Top16,
            8 => Quarterfinals,
            4 => Semifinals,
            2 => Final,
            _ => throw new ArgumentOutOfRangeException(nameof(numPlayers), "Parameter must be 2, 4, 8, 16, 32, or 64")
        };
    }

    /// <summary>
    /// Returns the players for the given tier.
    /// </summary>
    /// <param name="x">The top X players. Must be a power of 2.</param>
    /// <returns>Each player in no particular order</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if x does not reference a valid tier</exception>
    public IEnumerable<ParticipantViewModel> GetTopXParticipants(int x)
    {
        MatchGroupViewModel group = FindBracket(x);
        foreach (var match in group)
        {
            yield return match.First.Participant;
            yield return match.Second.Participant;
        }
    }

    /// <summary>
    /// Gets a participant's final placing in the bracket
    /// </summary>
    public int GetPlace(ParticipantViewModel participant)
    {
        if (participant == Final[0].Winner?.Participant)
            return 1;
        else if (participant == Final[0].Loser?.Participant)
            return 2;
        else if ( (Third[0].Winner is not null && participant == Third[0].Winner!.Participant) ||
                  (Third[0].Winner is null && Third[0].Contains(participant)))
            return 3;
        else if (Third[0].Loser is not null && participant == Third[0].Loser!.Participant)
            return 4;
        else if (Quarterfinals.Contains(participant))
            return 5;
        else if (Top16.Contains(participant))
            return 9;
        else if (Top32.Contains(participant))
            return 17;
        else return 33;
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

    #region Saber Sports

    /// <summary>
    /// Creates the json for submitting the tournament to saber-sports
    /// </summary>
    public JsonNode ToSaberSportsSubmission()
    {
        int id = 1;
        var rounds = new JsonArray();
        TimeSpan duration = TimeSpan.Zero;
        int score = 0;
        foreach (var round in EnumerateGroups())
        {
            if (round.IsCompleted)
            {
                var matches = new JsonArray();
                for (int i = 0; i < round.Matches.Count; i++)
                {
                    StandardMatchViewModel match = round.Matches[i] as StandardMatchViewModel ?? throw new InvalidOperationException("Can only submit standard matches to SaberScore.");
                    var fencers = new JsonArray();
                    if (match.First.Participant is PlayerViewModel firstPlayer)
                    {
                        fencers.Add(new JsonObject
                        {
                            ["uuid"] = firstPlayer.SaberSportId,
                            ["score"] = match.First.Points,
                            ["is_winner"] = match.IsFirstWinner,
                            ["actions"] = new JsonArray([.. match.FirstActions.ToSaberScore()])
                        });
                    }
                    if (match.Second.Participant is PlayerViewModel secondPlayer)
                    {
                        fencers.Add(new JsonObject
                        {
                            ["uuid"] = secondPlayer.SaberSportId,
                            ["score"] = match.Second.Points,
                            ["is_winner"] = match.IsSecondWinner,
                            ["actions"] = new JsonArray([.. match.SecondActions.ToSaberScore()])
                        });
                    }

                    var matchNode = new JsonObject
                    {
                        ["id"] = i,
                        ["fencers"] = fencers
                    };
                    matches.Add(matchNode);
                }
                rounds.Add(new JsonObject
                {
                    ["id"] = id++,
                    ["matches"] = matches
                });

                // find longest match and highest score for the configuration
                if (round.Settings.TimeLimit > duration)
                    duration = round.Settings.TimeLimit;
                if (round.Settings.WinningScore > score)
                    score = round.Settings.WinningScore;
            }
        }

        var config = new JsonObject();
        config["name"] = "Direct Elimination";
        config["duration"] = duration.ToString("mm\\:ss");
        config["score"] = score;
        config["disable_rank_promotion"] = false;

        var node = new JsonObject();
        node["type"] = "bracket";
        node["configuration"] = config;
        node["round"] = new JsonObject
        {
            ["rounds"] = rounds
        };

        return node;
    }

    #endregion
}
