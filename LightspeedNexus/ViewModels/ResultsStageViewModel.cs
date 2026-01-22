using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// The final statistics for a single player
/// </summary>
public partial class StatisticsViewModel : ViewModelBase
{
    #region Properties

    /// <summary>
    /// The placement of the player
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsGold))]
    [NotifyPropertyChangedFor(nameof(IsSilver))]
    [NotifyPropertyChangedFor(nameof(IsBronze))]
    public partial int Place { get; set; } = 0;

    /// <summary>
    /// The player
    /// </summary>
    [ObservableProperty]
    public partial ParticipantViewModel Participant { get; set; }

    /// <summary>
    /// The player's total wins
    /// </summary>
    [ObservableProperty]
    public partial int Wins { get; set; } = 0;

    /// <summary>
    /// The player's total points
    /// </summary>
    [ObservableProperty]
    public partial int Points { get; set; } = 0;

    /// <summary>
    /// The tournament value the player will have earned
    /// </summary>
    [ObservableProperty]
    public partial double Value { get; set; } = 0.0;

    /// <summary>
    /// The new rank
    /// </summary>
    [ObservableProperty]
    public partial Rank? OldRank { get; set; }

    /// <summary>
    /// The new rank
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPromoted))]
    public partial Rank? NewRank { get; set; }

    public bool IsPromoted => NewRank > OldRank;
    public bool IsGold => Place == 1;
    public bool IsSilver => Place == 2;
    public bool IsBronze => Place == 3;

    /// <summary>
    /// The total possible points a player could have gotten (internally maintained)
    /// </summary>
    private readonly double _possiblePoints;

    #endregion

    /// <summary>
    /// Creates a statline for the given player. This doesn't do the calculations. It
    /// really sets up an empty statline, but future calls to <see cref="AddMatch(double, MatchViewModel)"/>
    /// build the stats for the player.
    /// </summary>
    public StatisticsViewModel(ParticipantViewModel participant, int place, int possibleWins)
    {
        Place = place;
        OldRank = participant is PlayerViewModel p ? p.Rank : null;
        Participant = participant;
        _possiblePoints = (25 * possibleWins) + 100;
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public StatisticsViewModel(Statistics stats)
    {
        Place = stats.Place;
        Participant = StrongReferenceMessenger.Default.Send(new RequestParticipant(stats.Participant));
        Wins = stats.Wins;
        Points = stats.Points;
        _possiblePoints = stats.PossiblePoints;
        Value = stats.Value;
        OldRank = stats.OldRank;
        NewRank = stats.NewRank;
    }

    /// <summary>
    /// Gets the record representation of this view model
    /// </summary>
    public Statistics ToModel() => new()
    {
        Place = Place,
        Participant = Participant.Guid,
        Wins = Wins,
        Points = Points,
        PossiblePoints = _possiblePoints,
        Value = Value,
        OldRank = OldRank,
        NewRank = NewRank
    };

    /// <summary>
    /// Adds a match's stats to this player's statline
    /// </summary>
    public void AddMatch(double value, MatchViewModel match)
    {
        if (match.First is not null && match.First.Participant == Participant)
        {
            Points += match.First.Points;
            if (match.IsFirstWinner)
                Wins++;
        }
        else if (match.Second is not null && match.Second.Participant == Participant)
        {
            Points += match.Second.Points;
            if (match.IsSecondWinner)
                Wins++;
        }
        Value = Points / _possiblePoints * value;
    }
}

public partial class ResultsStageViewModel : StageViewModel
{
    #region Properties

    public ObservableCollection<StatisticsViewModel> Placements { get; set; } = [];

    /// <summary>
    /// When a stage represents the final stage of a tournament, this will be true
    /// </summary>
    public override bool IsTournamentCompleted => true;

    #endregion

    #region Commands

    /// <summary>
    /// Go to the Final Results Stage
    /// </summary>
    [RelayCommand]
    private static void Close() => StrongReferenceMessenger.Default.Send<SaveAndCloseMessage>();

    #endregion

    public ResultsStageViewModel() : base("Results")
    {
    }

    public override ResultsStage ToModel() => new()
    {
        Placements = [.. Placements.Select(s => s.ToModel())],
        Next = Next?.ToModel()
    };

    public static ResultsStageViewModel FromModel(ResultsStage model) => new()
    {
        Placements = [.. model.Placements.Select(s => new StatisticsViewModel(s))],
        Next = FromModel(model.Next)
    };

    public static ResultsStageViewModel FromBracket(BracketStageViewModel bracketStage)
    {
        var stage = new ResultsStageViewModel();

        var poolsStage = StrongReferenceMessenger.Default.Send<RequestPoolsStageMessage>().Response;
        var grades = StrongReferenceMessenger.Default.Send<RequestFinalGrading>().Response;
        var roster = StrongReferenceMessenger.Default.Send<RequestParticipants>().Response;
        var value = StrongReferenceMessenger.Default.Send<RequestTournamentValue>().Response;

        // initialize the dictionary with each player
        Dictionary<ParticipantViewModel, StatisticsViewModel> stats = [];
        int possibleWins = bracketStage.PossibleWins;
        foreach (var participant in roster)
        {
            if (participant is not null)
                stats[participant] = new StatisticsViewModel(participant, bracketStage.GetPlace(participant), possibleWins);
        }

        // process each pool match
        foreach (var pool in poolsStage.Pools)
            foreach (var match in pool.MatchGroup.Matches)
                ProcessMatch(stats, match, value);

        // process each bracket match
        foreach (var match in bracketStage.EnumerateMatches())
            ProcessMatch(stats, match, value);

        // set the stats
        foreach (var stat in stats.Values.OrderBy(s => s.Place).ThenBy(s => s.Points))
            stage.Placements.Add(stat);

        // upgrades
        if (grades is not null)
        {
            for (int i = 0; i < grades.Awards.Length && i < stage.Placements.Count; i++)
                stage.Placements[i].NewRank = grades.Awards[i];
        }

        return stage;
    }

    /// <summary>
    /// Updates the stats for the given player
    /// </summary>
    private static void ProcessMatch(Dictionary<ParticipantViewModel, StatisticsViewModel> stats, MatchViewModel match, int value)
    {
        if (match.HasBye || match.IsEmpty)
            return;

        if (stats.TryGetValue(match.First.Participant, out StatisticsViewModel? firstStats))
            firstStats.AddMatch(value, match);
        if (stats.TryGetValue(match.Second.Participant, out StatisticsViewModel? secondStats))
            secondStats.AddMatch(value, match);
    }
}
