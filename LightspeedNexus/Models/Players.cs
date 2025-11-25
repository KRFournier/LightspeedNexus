using System.Diagnostics.Metrics;

namespace LightspeedNexus.Models;

/// <summary>
/// Common interface for participants in a tournament, either individual contestants or teams.
/// </summary>
public abstract class Participant
{
    public string Name { get; set; } = string.Empty;
    public int PowerLevel { get; set; } = 0;

    public Participant() { }
    public Participant(string name, int powerLevel)
    {
        Name = name;
        PowerLevel = powerLevel;
    }

    public override string ToString() => $"{Name} - {PowerLevel}";
}

/// <summary>
/// Represents a bye
/// </summary>
public sealed class Bye : Participant
{
    public Bye() : base("BYE", 0) { }
    public override string ToString() => "--BYE--";
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

    public int StartingLife { get; set; } = 0;

    public Player() { }
    public Player(string name, int powerLevel, int? onlineId, string? club, Rank rank,
        Card card, int honor, int forceCalls, bool isEjected, WeaponClass weaponOfChoice)
        : base(name, powerLevel)
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
}

/// <summary>
/// A team, consisting of multiple members
/// </summary>
public sealed class Team : Participant
{
    public Player[] Members { get; set; } = [];

    public Team() { }
    public Team(string name, int powerLevel, Player[] members) : base(name, powerLevel)
    {
        Members = members;
    }
}