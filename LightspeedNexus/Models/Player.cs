using LightspeedNexus.ViewModels;
using System.Collections.Generic;

namespace LightspeedNexus.Models;

/// <summary>
/// Common interface for participants in a tournament, either individual contestants or teams.
/// </summary>
public abstract class Participant
{
    public string Name { get; set; } = string.Empty;
    public Participant() { }
    public Participant(string name)
    {
        Name = name;
    }
    public override string ToString() => Name;
}

/// <summary>
/// Represents an individual fencer in a tournament.
/// </summary>
public sealed class Player : Participant
{
    public int? OnlineId { get; set; } = null;
    public string? Club { get; set; }
    public Rank Rank { get; set; } = Rank.U;

    public Card Card { get; set; } = Card.None;
    public int Honor { get; set; } = 0;
    public int ForceCalls { get; set; } = 0;
    public bool IsEjected { get; set; } = false;
    public WeaponClass WeaponOfChoice { get; set; } = WeaponClass.Rey;

    public Player() { }
    public Player(string name, int? onlineId, string? club, Rank rank,
        Card card, int honor, int forceCalls, bool isEjected, WeaponClass weaponOfChoice)
        : base(name)
    {
        OnlineId = onlineId;
        Club = club;
        Rank = rank;
        Card = card;
        Honor = honor;
        ForceCalls = forceCalls;
        IsEjected = isEjected;
        WeaponOfChoice = weaponOfChoice;
    }

    public new PlayerViewModel ToViewModel() => new(this);
}

/// <summary>
/// A team, consisting of one or more players
/// </summary>
//public sealed class Team : Participant
//{
//    public Player[] Members { get; set; } = [];

//    public Team() { }
//    public Team(string name, Player[] members) : base(name)
//    {
//        Members = members;
//    }

//    public TeamViewModel ToViewModel(IReadOnlyList<PlayerViewModel> players) => new(this, players);
//};
