using LightspeedNexus.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

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
    Indirect,
    OutOfBounds,
    Overtime,
    Penalty,
    Priority,
    PriorityHeadshot,
    Return,
}

/// <summary>
/// An action performed by a player during a match.
/// </summary>
public sealed class Action
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int? Actor { get; set; }
    public int? Scorer { get; set; }
    public int Points { get; set; } = 0;
    public ActionType Type { get; set; } = ActionType.Unknown;
    public string? SubType { get; set; }

    public Action() { }

    public Action(Guid id, int? actor, int? scorer, int points, ActionType type, string? subType)
    {
        Id = id;
        Actor = actor;
        Scorer = scorer;
        Points = points;
        Type = type;
        SubType = subType;
    }

    //public ActionViewModel ToViewModel(IReadOnlyList<ContestantViewModel> players) => new(this, players);
}
