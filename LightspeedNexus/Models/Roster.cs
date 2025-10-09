using LightspeedNexus.Services;
using LightspeedNexus.ViewModels;
using LiteDB;
using System;
using System.Collections.Generic;
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
/// Represents a player in a tournment. Inherits Fighter but is not stored in the same collection.
/// This ensures that fighters can be deleted without affecting historical data.
/// </summary>
public sealed record Player(
    Guid Id,
    int? OnlineId,
    string FirstName,
    string LastName,
    string? Club,
    Rank Rey,
    Rank Ren,
    Rank Tano,
    Card Card,
    int Honor,
    int ForceCalls,
    bool IsEjected,
    WeaponClass WeaponOfChoice
) : Fighter(Id, OnlineId, FirstName, LastName, Club, Rey, Ren, Tano)
{
    public new PlayerViewModel ToViewModel() => new(this);
    public FighterViewModel ToFighterViewModel() => base.ToViewModel();
}

/// <summary>
/// A pool of players in a tournament.
/// </summary>
public sealed record Squadron(
    int[] Players,
    int Weight
)
{
    public SquadronViewModel ToViewModel(MatchSettingsViewModel poolSettings, IReadOnlyList<PlayerViewModel> fullRoster)
        => new(this, poolSettings, fullRoster);
}

/// <summary>
/// The players for a tournament
/// </summary>
public sealed record Roster(
    Player[] Players,
    Squadron[] Squadrons,
    bool IsStarted,
    bool IsAutoAssigned
)
{
    public RosterViewModel ToViewModel(SettingsViewModel settings) => new(settings);
}
