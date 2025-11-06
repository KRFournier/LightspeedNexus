using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightspeedNexus.ViewModels;

public partial class ActionViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial Guid Guid { get; protected set; } = Guid.NewGuid();

    [ObservableProperty]
    public partial ContestantViewModel? Actor { get; set; }

    [ObservableProperty]
    public partial ContestantViewModel? Scorer { get; set; }

    [ObservableProperty]
    public partial int Points { get; set; } = 0;

    [ObservableProperty]
    public partial ActionType Type { get; set; } = ActionType.Unknown;

    [ObservableProperty]
    public partial string? SubType { get; set; }

    #endregion

    /// <summary>
    /// Creates a brand new action
    /// </summary>
    public ActionViewModel() { }

    /// <summary>
    /// Loads an existing action
    /// </summary>
    public ActionViewModel(Models.Action action, IReadOnlyList<ContestantViewModel> players)
    {
        Guid = action.Id;
        Actor = action.Actor.HasValue ? players[action.Actor.Value] : null;
        Scorer = action.Scorer.HasValue ? players[action.Scorer.Value] : null;
        Points = action.Points;
        Type = action.Type;
        SubType = action.SubType;
    }

    /// <summary>
    /// Converts to an <see cref="Models.Action"/>.
    /// </summary>
    public Models.Action ToModel(IList<ContestantViewModel> players) => new(
        Guid,
        Actor is not null ? players.IndexOf(Actor) : null,
        Scorer is not null ? players.IndexOf(Scorer) : null,
        Points,
        Type,
        SubType
    );
}
