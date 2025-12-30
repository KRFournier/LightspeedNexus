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
/// Base class for all matches
/// </summary>
public abstract class Match : CollectionObject
{
    public bool IsMatchStarted { get; set; } = false;
}

/// <summary>
/// A standard match
/// </summary>
public sealed class StandardMatch : Match
{
    public Score First { get; set; } = new();
    public Score Second { get; set; } = new();
    public Side Winner { get; set; } = Side.Neither;
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
    public TimeSpan? SecondaryTimer { get; set; }
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

/// <summary>
/// A score for one side or the other. Score is abstract. The points could
/// be anything from life to action points, depending on the match.
/// </summary>
public sealed class Score
{
    /// <summary>
    /// A participant could be a player or a team.
    /// </summary>
    public Guid Participant { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The points for the player or team.
    /// </summary>
    public int Points { get; set; } = 0;
}

#endregion