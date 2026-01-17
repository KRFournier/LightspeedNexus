using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;

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
}
