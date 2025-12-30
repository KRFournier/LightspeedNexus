using System;

namespace LightspeedNexus.Models;

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
    HeadshotOverride,
    Indirect,
    OutOfBounds,
    Overtime,
    Penalty,
    Priority,
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
}
