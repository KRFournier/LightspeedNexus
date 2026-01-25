using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System.Collections.Generic;
using System.Linq;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A match
/// </summary>
public partial class StandardMatchViewModel : MatchViewModel
{
    #region Properties

    [ObservableProperty]
    public partial ClockViewModel Clock { get; set; } = new();

    [ObservableProperty]
    public partial Models.Action[] Actions { get; set; } = [];

    [ObservableProperty]
    public partial PriorityViewModel Priority { get; set; } = new();

    #endregion

    public override StandardMatch ToModel() => new()
    {
        Id = Guid,
        Number = Number,
        Clock = Clock.ToModel(),
        First = First.ToModel(),
        Second = Second.ToModel(),
        IsMatchStarted = IsMatchStarted,
        Actions = Actions,
        Priority = Priority.ToModel(),
        Winner = WinningSide
    };

    public static StandardMatchViewModel FromModel(StandardMatch model)
    {
        StandardMatchViewModel vm = new()
        {
            Guid = model.Id,
            Number = model.Number,
            Clock = ClockViewModel.FromModel(model.Clock),
            First = ScoreViewModel.FromModel(model.First),
            Second = ScoreViewModel.FromModel(model.Second),
            IsMatchStarted = model.IsMatchStarted,
            Actions = model.Actions,
            Priority = PriorityViewModel.FromModel(model.Priority),
            WinningSide = model.Winner
        };
        return vm;
    }

    #region Actions

    ///// <summary>
    ///// Adds the action and updates the player states
    ///// </summary>
    //public void SetNewAction(NewActionState state)
    //{
    //    TimeRemaining = state.TimeRemaining;
    //    OvertimeCount = state.OvertimeCount;

    //    var first = GetParticipant(state.First.Id);
    //    if (first is not null)
    //    {
    //        first.Score = state.First.Score;
    //        first.MinorViolations = state.First.MinorViolationCount;
    //        first.Player.Card = state.First.Card;
    //        first.Player.Ejected = state.First.Ejected;
    //        first.Player.Honor = state.First.Honor;
    //        first.Player.ForceCalls = state.First.ForceCalls;
    //    }

    //    var second = GetParticipant(state.Second.Id);
    //    if (second is not null)
    //    {
    //        second.Score = state.Second.Score;
    //        second.MinorViolations = state.Second.MinorViolationCount;
    //        second.Player.Card = state.Second.Card;
    //        second.Player.Ejected = state.Second.Ejected;
    //        second.Player.Honor = state.Second.Honor;
    //        second.Player.ForceCalls = state.Second.ForceCalls;
    //    }

    //    Actions.Insert(0, state.Action);
    //}

    ///// <summary>
    ///// Adds the action and updates the player states
    ///// </summary>
    //public void ModifyAction(ActionModified state)
    //{
    //    var first = GetParticipant(state.First.Id);
    //    first?.Score = state.First.Score;

    //    var second = GetParticipant(state.Second.Id);
    //    second?.Score = state.Second.Score;

    //    var action = Actions.FirstOrDefault(a => a.Id == state.ActionId);
    //    action?.Points = state.Points;
    //}

    ///// <summary>
    ///// Updates the players states and removes the last action
    ///// </summary>
    //public void UndoAction(UndoActionState state)
    //{
    //    TimeRemaining = state.TimeRemaining;
    //    OvertimeCount = state.OvertimeCount;

    //    var first = GetParticipant(state.First.Id);
    //    if (first is not null)
    //    {
    //        first.Score = state.First.Score;
    //        first.MinorViolations = state.First.MinorViolationCount;
    //        first.Player.Card = state.First.Card;
    //        first.Player.Ejected = state.First.Ejected;
    //        first.Player.Honor = state.First.Honor;
    //    }

    //    var second = GetParticipant(state.Second.Id);
    //    if (second is not null)
    //    {
    //        second.Score = state.Second.Score;
    //        second.MinorViolations = state.Second.MinorViolationCount;
    //        second.Player.Card = state.Second.Card;
    //        second.Player.Ejected = state.Second.Ejected;
    //        second.Player.Honor = state.Second.Honor;
    //    }

    //    var action = Actions.FirstOrDefault(a => a.Id == state.ActionId);
    //    if (action is not null)
    //        Actions.Remove(action);
    //}

    public IEnumerable<Action> FirstActions => Actions.Where(a => a.Actor == Side.First);
    public IEnumerable<Action> SecondActions => Actions.Where(a => a.Actor == Side.Second);

    #endregion
}
