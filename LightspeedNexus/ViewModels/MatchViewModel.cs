using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A participant(s)' score in a match
/// </summary>
public partial class ScoreViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial ParticipantViewModel Participant { get; set; }

    [ObservableProperty]
    public partial int Points { get; set; } = 0;

    /// <summary>
    /// Determines if this player/team is out of the match, either due to disqualification or
    /// because this score represents a bye
    /// </summary>
    public bool IsOut => Participant.IsDisqualified;

    #endregion

    public ScoreViewModel()
    {
        Participant = new PlayerViewModel();
    }

    public ScoreViewModel(ParticipantViewModel participant) 
    {
        Participant = participant;
    }

    public ScoreViewModel(Score score, IReadOnlyList<ParticipantViewModel> participants)
    {
        Points = score.Points;
        Participant = participants[score.Participant];
    }

    public Score ToModel(IList<ParticipantViewModel> participants) => new(
        participants.IndexOf(Participant),
        Points
    );
}

/// <summary>
/// A match
/// </summary>
public partial class MatchViewModel : ViewModelBase
{
    #region Properties

    public Guid Guid { get; protected set; } = Guid.NewGuid();

    [ObservableProperty]
    public partial MatchSettingsViewModel Settings { get; set; } = new();

    [ObservableProperty]
    public partial int? Number { get; set; }

    [ObservableProperty]
    public partial bool IsLive { get; set; } = false;

    [ObservableProperty]
    public partial TimeSpan TimeRemaining { get; set; } = TimeSpan.FromSeconds(90);

    [ObservableProperty]
    public partial bool IsMatchStarted { get; set; } = false;

    [ObservableProperty]
    public partial int OvertimeCount { get; set; } = 0;

    [ObservableProperty]
    public partial ScoreViewModel? First { get; set; }

    [ObservableProperty]
    public partial ScoreViewModel? Second { get; set; }

    [ObservableProperty]
    public partial Models.Action[] Actions { get; set; } = [];

    [ObservableProperty]
    public partial Side PreviousPriority { get; set; } = Side.Neither;

    [ObservableProperty]
    public partial int PriorityPoints { get; set; } = 3;

    [ObservableProperty]
    public partial bool InPriority { get; set; } = false;

    #endregion

    public MatchViewModel()
    {
    }

    public MatchViewModel(MatchSettingsViewModel settings)
    {
        Settings = settings;
    }

    public MatchViewModel(ParticipantViewModel first, ParticipantViewModel second, MatchSettingsViewModel settings) : this(settings)
    {
        First = new ScoreViewModel(first);
        Second = new ScoreViewModel(second);
    }

    public MatchViewModel(Match match, MatchSettingsViewModel settings, IReadOnlyList<ParticipantViewModel> participants) : this(settings)
    {
        Guid = match.Id;
        TimeRemaining = match.TimeRemaining;
        OvertimeCount = match.OvertimeCount;
        First = match.First is not null ? new ScoreViewModel(match.First, participants) : null;
        Second = match.Second is not null ? new ScoreViewModel(match.Second, participants) : null;
        IsMatchStarted = match.IsMatchStarted;
        Actions = match.Actions;
        PreviousPriority = match.PreviousPriority;
        PriorityPoints = match.PriorityPoints;
        InPriority = match.InPriority;
    }

    public Match ToModel(IList<ParticipantViewModel> participants) => new(
        Guid,
        TimeRemaining,
        OvertimeCount,
        First?.ToModel(participants),
        Second?.ToModel(participants),
        IsMatchStarted,
        Actions,
        PreviousPriority,
        PriorityPoints,
        InPriority
        );

    #region Winner

    /// <summary>
    /// The winner of the match, if there is one
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Loser))]
    [NotifyPropertyChangedFor(nameof(IsMatchCompleted))]
    [NotifyPropertyChangedFor(nameof(WinnerScore))]
    [NotifyPropertyChangedFor(nameof(LoserScore))]
    [NotifyPropertyChangedFor(nameof(IsFirstWinner))]
    [NotifyPropertyChangedFor(nameof(IsSecondWinner))]
    public partial ScoreViewModel? Winner { get; set; } = null;

    /// <summary>
    /// The loser of the match
    /// </summary>
    public ScoreViewModel? Loser => Winner switch
    {
        _ when Winner == First => Second,
        _ when Winner == Second => First,
        _ => null
    };

    /// <summary>
    /// Determines if the winner is first
    /// </summary>
    public bool IsFirstWinner => Winner == First;

    /// <summary>
    /// Determines if the winner is red
    /// </summary>
    public bool IsSecondWinner => Winner == Second;

    /// <summary>
    /// The winner's score
    /// </summary>
    public int WinnerScore => IsFirstWinner ? First!.Points : (IsSecondWinner ? Second!.Points : 0);

    /// <summary>
    /// The loser's score
    /// </summary>
    public int LoserScore => IsFirstWinner ? Second!.Points : (IsSecondWinner ? First!.Points : 0);

    /// <summary>
    /// Determines if the match is completed based on whether or not there is a winner
    /// </summary>
    public bool IsMatchCompleted => Winner is not null;

    /// <summary>
    /// Updates the Winner
    /// </summary>
    private void CheckWinners()
    {
        if (First is null || Second is null)
            return;

        // check for autowins
        if (Second.IsOut && !First.IsOut)
        {
            Winner = First;
        }
        else if (First.IsOut && !Second.IsOut)
        {
            Winner = Second;
        }
        else
        {
            if (First.Points == Second.Points)
                Winner = null;
            else
            {
                // check for winner at end of time
                if (TimeRemaining == TimeSpan.Zero)
                {
                    TimeRemaining = TimeSpan.Zero;
                    if (First.Points > Second.Points)
                        Winner = First;
                    else if (Second.Points > First.Points)
                        Winner = Second;
                }

                // check for winning score
                else
                {
                    if (First.Points >= Settings.WinningScore)
                        Winner = First;
                    else if (Second.Points >= Settings.WinningScore)
                        Winner = Second;
                }
            }
        }

        OnPropertyChanged(nameof(IsFirstWinner));
        OnPropertyChanged(nameof(IsSecondWinner));
        OnPropertyChanged(nameof(WinnerScore));
        OnPropertyChanged(nameof(LoserScore));
    }

    #endregion
}
