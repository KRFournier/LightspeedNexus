using System;

namespace LightspeedNexus.Models;

/// <summary>
/// Common interface for participants in a tournament, either individual contestants or teams.
/// </summary>
public abstract class Participant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int PowerLevel { get; set; } = 0;

    public override string ToString() => $"{Name} - {PowerLevel}";
}

/// <summary>
/// Represents a bye, i.e., a placeholder participant that automatically loses.
/// </summary>
public sealed class Bye : Participant
{
    public Bye()
    {
        Id = Guid.Empty;
        Name = "BYE";
    }

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
}

/// <summary>
/// A team, consisting of multiple members
/// </summary>
public sealed class Team : Participant
{
    public Player[] Members { get; set; } = [];
}