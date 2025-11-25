using System;

namespace LightspeedNexus.Models;

/// <summary>
/// Identifies a participant the side to which they were assigned
/// </summary>
public enum Side
{
    Neither,
    First,
    Second
}

/// <summary>
/// Specifies the type of scoring actions possible in a match.
/// </summary>
public enum ActionType
{
    Unknown = -1,
    Card,
    Clean,
    Conceded,
    Disarm,
    Ejected,
    FirstContact,
    Headshot,
    Indirect,
    OutOfBounds,
    Overtime,
    Penalty,
    Priority,
    PriorityHeadshot,
    Return,
}

/// <summary>
/// An action performed in a match.
/// </summary>
public sealed class Action
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Side Actor { get; set; } = Side.Neither;
    public Side Scorer { get; set; } = Side.Neither;
    public int Points { get; set; } = 0;
    public ActionType Type { get; set; } = ActionType.Unknown;
    public string? SubType { get; set; }

    public Action() { }

    public Action(Guid id, Side actor, Side scorer, int points, ActionType type, string? subType)
    {
        Id = id;
        Actor = actor;
        Scorer = scorer;
        Points = points;
        Type = type;
        SubType = subType;
    }
}

/// <summary>
/// A score for one side or the other
/// </summary>
public sealed class Score
{
    public int Participant { get; set; } = 0;
    public int Points { get; set; } = 0;
    public Score() { }
    public Score(int participants, int points)
    {
        Participant = participants;
        Points = points;
    }
}

/// <summary>
/// A match
/// </summary>
public class Match : CollectionObject
{
    public TimeSpan TimeRemaining { get; set; } = TimeSpan.FromSeconds(90);
    public bool IsMatchStarted { get; set; } = false;

    public int OvertimeCount { get; set; } = 0;

    public Score? First { get; set; }
    public Score? Second { get; set; }

    public Action[] Actions { get; set; } = [];

    public Side PreviousPriority { get; set; } = Side.Neither;
    public int PriorityPoints { get; set; } = 3;
    public bool InPriority { get; set; } = false;

    public Match() { }

    public Match(Guid id, TimeSpan timeRemaining, int overtimeCount,
        Score? first, Score? second, bool isMatchStarted, Action[] actions, Side previousPriority,
        int priorityPoints, bool inPriority) : base(id)
    {
        TimeRemaining = timeRemaining;
        OvertimeCount = overtimeCount;
        First = first;
        Second = second;
        IsMatchStarted = isMatchStarted;
        Actions = actions;
        PreviousPriority = previousPriority;
        PriorityPoints = priorityPoints;
        InPriority = inPriority;
    }
}
