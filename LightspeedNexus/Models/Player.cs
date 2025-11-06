using LightspeedNexus.ViewModels;
using System;
using System.Collections.Generic;

namespace LightspeedNexus.Models;

/// <summary>
/// Common interface for participants in a tournament, either individual contestants or teams.
/// </summary>
public interface IParticipant
{
    Guid Id { get; set; }
    string? Name { get; }
}

/// <summary>
/// Represents an individual fencer in a tournament. Inherits Fighter but is not stored in the same collection.
/// This ensures that fighters can be deleted without affecting historical data.
/// </summary>
public sealed class Player : Fighter, IParticipant
{
    public Card Card { get; set; } = Card.None;
    public int Honor { get; set; } = 0;
    public int ForceCalls { get; set; } = 0;
    public bool IsEjected { get; set; } = false;
    public WeaponClass WeaponOfChoice { get; set; } = WeaponClass.Rey;

    public Player() { }
    public Player(Guid id, int? onlineId, string firstName, string lastName, string? club, Rank rey, Rank ren, Rank tano,
        Card card, int honor, int forceCalls, bool isEjected, WeaponClass weaponOfChoice)
        : base(id, onlineId, firstName, lastName, club, rey, ren, tano)
    {
        Card = card;
        Honor = honor;
        ForceCalls = forceCalls;
        IsEjected = isEjected;
        WeaponOfChoice = weaponOfChoice;
    }

    public new PlayerViewModel ToViewModel() => new(this);
    public FighterViewModel ToFighterViewModel() => base.ToViewModel();
}

///// <summary>
///// A team, consisting of one or more players
///// </summary>
//public sealed class Team : CollectionObject, IParticipant
//{
//    public string? Name { get; set; }
//    public Player[] Members { get; set; } = [];

//    public Team() { }

//    public Team(Guid id, string? name, Player[] members) : base(id)
//    {
//        Name = name;
//        Members = members;
//    }

//    public TeamViewModel ToViewModel(IReadOnlyList<PlayerViewModel> players) => new(this, players);

//    public override string ToString() => Name ?? "";
//};
