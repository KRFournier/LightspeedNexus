using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A match
/// </summary>
public partial class PoolMatchViewModel : MatchViewModel
{
    /// <summary>
    /// The squadron to which this match belongs
    /// </summary>
    [ObservableProperty]
    public partial SquadronViewModel? Squadron { get; protected set; }

    /// <summary>
    /// Creates a brand new pool match with the given squadron
    /// </summary>
    public PoolMatchViewModel(SquadronViewModel squadron) : base()
    {
        Squadron = squadron;
    }

    /// <summary>
    /// Loads an existing match with the given squadron
    /// </summary>
    public PoolMatchViewModel(SquadronViewModel squadron, Match match, IReadOnlyList<ContestantViewModel> fullRoster) : base(match, fullRoster)
    {
        Squadron = squadron;
    }

    /// <summary>
    /// Creates a match with the given players
    /// </summary>
    public PoolMatchViewModel(int number, SquadronViewModel squadron, TimeSpan timeLimit, ContestantViewModel? playerOne, ContestantViewModel? playerTwo)
    {
        timer.Elapsed += OnTimerTick;
        Number = number;
        PlayerOne = playerOne is not null ? new TeamViewModel(playerOne) : null;
        PlayerTwo = playerTwo is not null ? new TeamViewModel(playerTwo) : null;
        Squadron = squadron;
        Settings = squadron.Settings;
        TimeRemaining = TimeLimit = timeLimit;
        CheckWinners();
    }
}
