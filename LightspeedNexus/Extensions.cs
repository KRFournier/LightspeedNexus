using LightspeedNetwork;
using LightspeedNexus.Models;
using System.Collections.Generic;
using System.Linq;

namespace LightspeedNexus;

public static class Extensions
{
    public static string? ToSaberScore(this ActionType type) => type switch
    {
        ActionType.Card => "T",
        ActionType.Clean => "C",
        ActionType.Conceded => "H",
        ActionType.Disarm => "D",
        ActionType.FirstContact => "F",
        ActionType.Headshot => "A",
        ActionType.Indirect => "I",
        ActionType.OutOfBounds => "O",
        ActionType.Penalty => "S",
        ActionType.Priority => "P",
        ActionType.HeadshotOverride => "V",
        _ => null,
    };

    public static IEnumerable<string> ToSaberScore(this IEnumerable<Action> actions) => actions
            .Select(a => a.Type.ToSaberScore())
            .Where(s => s != null)!;

    public static ActionState ToState(this Action action) => new()
    {
        Id = action.Id,
        Actor = action.Actor,
        Scorer = action.Scorer,
        Points = action.Points,
        Type = action.Type,
        SubType = action.SubType
    };

    public static Action ToModel(this ActionState state) => new()
    {
        Id = state.Id,
        Actor = state.Actor,
        Scorer = state.Scorer,
        Points = state.Points,
        Type = state.Type,
        SubType = state.SubType
    };
}
