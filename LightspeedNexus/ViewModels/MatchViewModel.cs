using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A match
/// </summary>
public partial class MatchViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial Guid Guid { get; protected set; } = Guid.NewGuid();

    [ObservableProperty]
    public partial bool IsLive { get; protected set; } = false;

    [ObservableProperty]
    public partial int Number { get; set; } = 0;

    [ObservableProperty]
    public partial LocalMatchSettingsViewModel Settings { get; set; }

    partial void OnSettingsChanged(MatchSettingsViewModel oldValue, MatchSettingsViewModel newValue)
    {
        oldValue.PropertyChanged -= OnSettingsPropertyChanged;
        newValue.PropertyChanged += OnSettingsPropertyChanged;
    }
    protected void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MatchSettingsViewModel.WinningScore))
        {
            OnPropertyChanged(nameof(IsPlayerOneWinner));
            OnPropertyChanged(nameof(IsPlayerTwoWinner));
            OnPropertyChanged(nameof(IsMatchCompleted));
            CheckWinners();
        }
        else if (e.PropertyName == nameof(MatchSettingsViewModel.TimeLimit))
        {
            TimeRemaining = Settings.TimeLimit;
        }
    }

    [ObservableProperty]
    public partial bool IsMatchStarted { get; set; } = false;

    #endregion

    /// <summary>
    /// Creates a brand new match
    /// </summary>
    public MatchViewModel()
    {
        TimeRemaining = Settings.TimeLimit;
    }

    /// <summary>
    /// Loads an existing match
    /// </summary>
    public MatchViewModel(Match match, IReadOnlyList<ContestantViewModel> fullRoster)
    {
        Guid = match.Id;
        Number = match.Number;
        Settings = new LocalMatchSettingsViewModel(match.Settings);
        TimeRemaining = match.TimeRemaining;
        OvertimeCount = match.OvertimeCount;
        Players = [.. match.Players.Select(i => fullRoster[i])];
        PlayerOne = match.PlayerOne is not null ? new TeamViewModel(match.PlayerOne, Players) : null;
        PlayerTwo = match.PlayerTwo is not null ? new TeamViewModel(match.PlayerTwo, Players) : null;
        IsMatchStarted = match.IsMatchStarted;
        Actions = [.. match.Actions.Select(a => new ActionViewModel(a, Players))];
        PreviousPriority = GetPriorityPlayer(match.PreviousPriority);
        PriorityPoints = match.PriorityPoints;
        InPriority = match.InPriority;
    }

    /// <summary>
    /// Converts to a <see cref="Match"/>.
    /// </summary>
    public Match ToModel(IList<ContestantViewModel> fullRoster) => new(
        Guid,
        Number,
        Settings.ToModel(),
        TimeRemaining,
        OvertimeCount,
        [.. Players.Select(p => fullRoster.IndexOf(p))],
        PlayerOne?.ToModel(Players),
        PlayerTwo?.ToModel(Players),
        IsMatchStarted,
        [.. Actions.Select(a => a.ToModel(Players))],
        GetPriorityState(PreviousPriority),
        PriorityPoints,
        InPriority
    );

    #region Players

    /// <summary>
    /// All the players in the match
    /// </summary>
    public ObservableCollection<ContestantViewModel> Players { get; set; } = [];

    /// <summary>
    /// The first player/team
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMatchCompleted))]
    public partial TeamViewModel? PlayerOne { get; set; }
    partial void OnPlayerOneChanged(TeamViewModel? oldValue, TeamViewModel? newValue)
    {
        oldValue?.PropertyChanged -= OnScorePropertyChanged;
        newValue?.PropertyChanged += OnScorePropertyChanged;
    }

    /// <summary>
    /// The second player/team
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMatchCompleted))]
    public partial TeamViewModel? PlayerTwo { get; set; }
    partial void OnPlayerTwoChanged(TeamViewModel? oldValue, TeamViewModel? newValue)
    {
        oldValue?.PropertyChanged -= OnScorePropertyChanged;
        newValue?.PropertyChanged += OnScorePropertyChanged;
    }

    /// <summary>
    /// Handles changes to the score-related property and triggers winner evaluation when the points property changes.
    /// </summary>
    protected void OnScorePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TeamViewModel.Points))
            CheckWinners();
    }

    /// <summary>
    /// Determines if both players are not null
    /// </summary>
    public bool HasBothPlayers => PlayerOne is not null && PlayerTwo is not null;

    #endregion

    #region Winner

    /// <summary>
    /// The winner of the match, if there is one
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Loser))]
    [NotifyPropertyChangedFor(nameof(IsMatchCompleted))]
    [NotifyPropertyChangedFor(nameof(IsPlayerOneWinner))]
    [NotifyPropertyChangedFor(nameof(IsPlayerTwoWinner))]
    public partial TeamViewModel? Winner { get; protected set; }

    /// <summary>
    /// The loser of the match
    /// </summary>
    public TeamViewModel? Loser
    {
        get
        {
            if (Winner == PlayerOne)
                return PlayerTwo;
            else if (Winner == PlayerTwo)
                return PlayerOne;
            return null;
        }
    }

    /// <summary>
    /// Determines if the winner is player one
    /// </summary>
    public bool IsPlayerOneWinner => Winner == PlayerOne;

    /// <summary>
    /// Determines if the winner is player two
    /// </summary>
    public bool IsPlayerTwoWinner => Winner == PlayerTwo;

    /// <summary>
    /// Determines if the match is completed based on whether or not there is a winner
    /// </summary>
    public bool IsMatchCompleted => Winner is not null;

    /// <summary>
    /// Updates the Winner
    /// </summary>
    private void CheckWinners()
    {
        if (PlayerOne is null && PlayerTwo is null)
        {
            Winner = null;
            return;
        }

        bool hasFirstPlayer = !IsPlayerAbsent(PlayerOne);
        bool hasSecondPlayer = !IsPlayerAbsent(PlayerTwo);

        // check for autowins
        if (!hasFirstPlayer && !hasSecondPlayer)
        {
            Winner = null;
        }
        else if (!hasSecondPlayer && hasFirstPlayer)
        {
            Winner = PlayerOne;
        }
        else if (!hasFirstPlayer && hasSecondPlayer)
        {
            Winner = PlayerTwo;
        }
        else
        {
            if (PlayerOne!.Points == PlayerTwo!.Points)
                Winner = null;
            else
            {
                // check for winner at end of time
                if (TimeRemaining == TimeSpan.Zero)
                {
                    TimeRemaining = TimeSpan.Zero;
                    if (PlayerOne.Points > PlayerTwo.Points)
                        Winner = PlayerOne;
                    else if (PlayerTwo.Points > PlayerOne.Points)
                        Winner = PlayerTwo;
                }

                // check for winning score
                else
                {
                    if (PlayerOne.Points >= Settings.WinningScore)
                        Winner = PlayerOne;
                    else if (PlayerTwo.Points >= Settings.WinningScore)
                        Winner = PlayerTwo;
                }
            }
        }
    }

    private static bool IsPlayerAbsent(TeamViewModel? player) => player is null || player.IsDisqualified();

    #endregion

    #region Actions

    /// <summary>
    /// The actions submitted by the score keeper
    /// </summary>
    public ObservableCollection<ActionViewModel> Actions { get; set; } = [];

    #endregion

    #region Timer

    /// <summary>
    /// The timer
    /// </summary>
    private readonly Timer timer = new(1000);

    /// <summary>
    /// The time remaining
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsTimeUp))]
    public partial TimeSpan TimeRemaining { get; set; } = TimeSpan.FromSeconds(90);
    partial void OnTimeRemainingChanged(TimeSpan value) => CheckWinners();
    public bool IsTimeUp => TimeRemaining == TimeSpan.Zero;

    /// <summary>
    /// Indicates whether we are fencing!
    /// </summary>
    [ObservableProperty]
    public partial bool IsRunning { get; set; } = false;
    partial void OnIsRunningChanged(bool oldValue, bool newValue)
    {
        if (oldValue && !newValue)
            timer.Stop();
        else if (!oldValue && newValue)
            timer.Start();
    }

    /// <summary>
    /// One second
    /// </summary>
    private static readonly TimeSpan One = new(0, 0, 1);

    /// <summary>
    /// Adds one second
    /// </summary>
    [RelayCommand]
    private void AddTime() => TimeRemaining += One;

    /// <summary>
    /// Subtracts one second
    /// </summary>
    [RelayCommand]
    private void SubtractTime()
    {
        if (TimeRemaining > One)
            TimeRemaining -= One;
        else
            TimeRemaining = TimeSpan.Zero;
    }

    /// <summary>
    /// Handles the timer tick
    /// </summary>
    private void OnTimerTick(object? source, ElapsedEventArgs e)
    {
        TimeRemaining -= One;
        if (TimeRemaining <= TimeSpan.Zero)
            IsRunning = false;
    }

    /// <summary>
    /// The current overtime we are in
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NextOvertimeName))]
    public partial int OvertimeCount { get; set; } = 0;
    public string NextOvertimeName => OvertimeCount switch
    {
        0 => "Overtime",
        1 => "Second Overtime",
        _ => "Priority Overtime"
    };

    #endregion

    #region Priority

    [ObservableProperty]
    public partial TeamViewModel? PreviousPriority { get; set; }

    [ObservableProperty]
    public partial int PriorityPoints { get; set; } = 3;

    [ObservableProperty]
    public partial bool InPriority { get; set; } = false;

    protected PriorityState GetPriorityState(TeamViewModel? priority) =>
        priority == PlayerOne ? PriorityState.PlayerOne :
        priority == PlayerTwo ? PriorityState.PlayerTwo :
        PriorityState.None;

    protected TeamViewModel? GetPriorityPlayer(PriorityState state) => state switch {
        PriorityState.PlayerOne => PlayerOne,
        PriorityState.PlayerTwo => PlayerTwo,
        _ => null
    };

    #endregion
}
