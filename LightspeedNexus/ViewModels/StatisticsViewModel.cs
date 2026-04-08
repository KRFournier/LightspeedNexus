using CommunityToolkit.Mvvm.ComponentModel;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// The final statistics for a single player
/// </summary>
public partial class StatisticsViewModel : ObservableObject
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
    /// The total possible points a player could have gotten
    /// </summary>
    [ObservableProperty]
    public partial double PossiblePoints { get; set; } = 0;

    #endregion

    /// <summary>
    /// Gets the record representation of this view model
    /// </summary>
    public Statistics ToModel() => new()
    {
        Place = Place,
        Participant = Participant.Guid,
        Wins = Wins,
        Points = Points,
        PossiblePoints = PossiblePoints,
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
        Value = Points / PossiblePoints * value;
    }
}
