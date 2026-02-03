using LightspeedNetwork;
using System;

namespace LightspeedNexus.Models;

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

public static class ActionPointValues
{
    public const int WhiteCard = 0;
    public const int YellowCard = 3;
    public const int RedCard = 3;
    public const int Clean = 3;
    public const int Conceded = 1;
    public const int Disarm = 3;
    public const int FirstContact = 1;
    public const int Headshot = 3;
    public const int HeadshotOverride = 1;
    public const int Indirect = 1;
    public const int OutOfBounds = 3;
    public const int FirstMinorViolation = 0;
    public const int SubsequentMinorViolation = 3;
    public const int Priority = 3;
    public const int Return = 5;

    public static int Max => Return;
}
