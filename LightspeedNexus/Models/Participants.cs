using LiteDB;
using System;

namespace LightspeedNexus.Models;

/// <summary>
/// Common interface for participants in a tournament, either individual contestants or teams.
/// </summary>
public interface IParticipant
{
    public Guid Id { get; }
    public string Name { get; }
}

/// <summary>
/// Represents a bye, i.e., a placeholder participant that automatically loses.
/// </summary>
public sealed class ByeParticipant : IParticipant
{
    public readonly static Guid ByeGuid = new(0xffffffff, 0xffff, 0xffff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff);
    public Guid Id => ByeGuid;
    public string Name => "BYE";
}

/// <summary>
/// Placeholder participant used when a participant is expected but not yet assigned
/// </summary>
public sealed class EmptyParticipant : IParticipant
{
    public Guid Id => Guid.Empty;
    public string Name => "EMPTY";
}

/// <summary>
/// Represents an individual fencer in a tournament.
/// </summary>
public sealed class Player : IParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public int? OnlineId { get; set; } = null;
    public string? Club { get; set; }
    public Rank Rank { get; set; } = Rank.U;

    public Card Card { get; set; } = Card.None;
    public int Honor { get; set; } = 0;
    public int ForceCalls { get; set; } = 0;
    public bool IsEjected { get; set; } = false;
    public WeaponClass WeaponOfChoice { get; set; } = WeaponClass.Rey;

    public int StartingLife { get; set; } = 0;

    [BsonIgnore]
    public string Name => $"{FirstName} {LastName}";
}

/// <summary>
/// A team, consisting of multiple members
/// </summary>
public sealed class Team : IParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Player[] Members { get; set; } = [];
}