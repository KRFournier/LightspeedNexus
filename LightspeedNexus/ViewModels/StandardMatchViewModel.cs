using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Network;
using LightspeedNexus.Networking;
using System.Collections.ObjectModel;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A match
/// </summary>
public partial class StandardMatchViewModel : MatchViewModel
{
    #region Properties

    [ObservableProperty]
    public partial ClockViewModel Clock { get; set; } = new();

    public ObservableCollection<Lightspeed.Action> Actions { get; set; } = [];

    [ObservableProperty]
    public partial PriorityViewModel Priority { get; set; } = new();

    #endregion

    public StandardMatchViewModel() : base()
    {
        if (!Design.IsDesignMode)
        {
            WeakReferenceMessenger.Default.Register<ClockStateMessage, Guid>(this, Guid, (_, m) => Clock.FromState(m.State));
            WeakReferenceMessenger.Default.Register<NewActionMessage, Guid>(this, Guid, (_, m) => SetNewAction(m.State));
            WeakReferenceMessenger.Default.Register<UndoActionMessage, Guid>(this, Guid, (_, m) => UndoAction(m.State));
            WeakReferenceMessenger.Default.Register<ActionModifiedMessage, Guid>(this, Guid, (_, m) => ModifyAction(m.State));
            WeakReferenceMessenger.Default.Register<PriorityChangedMessage, Guid>(this, Guid, (_, m) => Priority.FromState(m.State.Priority));
        }
    }

    public override StandardMatch ToModel() => new()
    {
        Id = Guid,
        Number = Number,
        Clock = Clock.ToModel(),
        First = First.ToModel(),
        Second = Second.ToModel(),
        IsMatchStarted = IsMatchStarted,
        Actions = [.. Actions],
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
            Actions = [.. model.Actions],
            Priority = PriorityViewModel.FromModel(model.Priority),
            WinningSide = model.Winner
        };
        return vm;
    }

    public override StandardMatchState ToState() => new()
    {
        Id = Guid,
        First = First.ToState(),
        Second = Second.ToState(),
        Settings = Settings.ToState(),
        Clock = Clock.ToState(),
        Actions = [.. Actions.Select(a => a.ToState())],
        Priority = Priority.ToState()
    };


    public override MatchSummary ToSummary() => new()
    {
        Id = Guid,
        Number = Number ?? 0,
        First = First.ToState(),
        Second = Second.ToState(),
        Winner = WinningSide,
        IsStarted = IsMatchStarted,
        IsCompleted = IsMatchCompleted,
        Clock = Clock.ToState()
    };

    #region Actions

    /// <summary>
    /// Adds the action and updates the player states
    /// </summary>
    public void SetNewAction(NewActionState state)
    {
        Clock.FromState(state.Clock);
        First.FromState(state.First);
        Second.FromState(state.Second);

        if (state.Action is not null)
            Actions.Insert(0, state.Action.ToModel());
    }

    /// <summary>
    /// Adds the action and updates the player states
    /// </summary>
    public void ModifyAction(ActionModified state)
    {
        First.Points = state.Points;
        Second.Points = state.Points;

        var action = Actions.FirstOrDefault(a => a.Id == state.ActionId);
        action?.Points = state.Points;
    }

    /// <summary>
    /// Updates the players states and removes the last action
    /// </summary>
    public void UndoAction(UndoActionState state)
    {
        Clock.FromState(state.Clock);
        First.FromState(state.First);
        Second.FromState(state.Second);

        var action = Actions.FirstOrDefault(a => a.Id == state.ActionId);
        if (action is not null)
            Actions.Remove(action);
    }

    public IEnumerable<Lightspeed.Action> FirstActions => Actions.Where(a => a.Actor == Side.First);
    public IEnumerable<Lightspeed.Action> SecondActions => Actions.Where(a => a.Actor == Side.Second);

    #endregion

}
