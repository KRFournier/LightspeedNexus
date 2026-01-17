using System;

namespace LightspeedNexus.Models;

/// <summary>
/// Identifies a participant's side in the match.
/// </summary>
public enum Side
{
    Neither,
    First,
    Second
}

/// <summary>
/// References a participant in a match based on outcome.
/// </summary>
public enum MatchOutcome
{
    Winner,
    Loser
}

/// <summary>
/// Base class for all matches
/// </summary>
public abstract class Match : CollectionObject
{
    public int? Number { get; set; }
    public Score First { get; set; } = new();
    public Score Second { get; set; } = new();
    public Side Winner { get; set; } = Side.Neither;
    public bool IsMatchStarted { get; set; } = false;
}

/// <summary>
/// A standard match
/// </summary>
public sealed class StandardMatch : Match
{
    public Clock Clock { get; set; } = new();
    public Action[] Actions { get; set; } = [];
    public Priority Priority { get; set; } = new();
}

#region Components

/// <summary>
/// A clock used in a match. A clock can have multiple timers, but
/// one overtime counter.
/// </summary>
public sealed class Clock
{
    public TimeSpan Timer { get; set; } = TimeSpan.FromSeconds(90);
    public int Overtime { get; set; } = 0;
}

/// <summary>
/// Adds priority tracking to a match
/// </summary>
public sealed class Priority
{
    public Side PreviousPriority { get; set; } = Side.Neither;
    public int PriorityPoints { get; set; } = 3;
    public bool InPriority { get; set; } = false;
}

#endregion