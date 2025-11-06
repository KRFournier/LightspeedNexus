using LightspeedNexus.Services;
using LightspeedNexus.ViewModels;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LightspeedNexus.Models;

/// <summary>
/// The possible cards a player can receive during a match.
/// </summary>
public enum Card
{
    None,
    White,
    Yellow,
    Red,
    Black
}

/// <summary>
/// A pool of players in a tournament.
/// </summary>
public sealed class Squadron
{
    public int[] Players { get; set; } = [];
    public int Weight { get; set; } = 0;

    public Squadron() { }
    public Squadron(int[] players, int weight)
    {
        Players = players;
        Weight = weight;
    }

    public SquadronViewModel ToViewModel(MatchSettingsViewModel poolSettings, IReadOnlyList<ContestantViewModel> fullRoster)
        => new(this, poolSettings, fullRoster);
}

/// <summary>
/// The players for a tournament
/// </summary>
public sealed class Roster
{
    public Contestant[] Players { get; set; } = [];
    public Squadron[] Squadrons { get; set; } = [];
    public bool IsStarted { get; set; } = false;
    public bool IsAutoAssigned { get; set; } = true;

    public Roster() { }
    public Roster(Contestant[] players, Squadron[] squadrons, bool isStarted, bool isAutoAssigned)
    {
        Players = players;
        Squadrons = squadrons;
        IsStarted = isStarted;
        IsAutoAssigned = isAutoAssigned;
    }

    public RosterViewModel ToViewModel(SettingsViewModel settings) => new(settings);
}
