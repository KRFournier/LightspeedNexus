using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

public partial class StageItem(string name, bool isActive = false) : ViewModelBase
{
    [ObservableProperty]
    public partial bool IsActive { get; set; } = isActive;

    [ObservableProperty]
    public partial string Name { get; set; } = name;
}

public partial class TournamentViewModel : ViewModelBase
{
    #region Properties

    public ObservableCollection<StageItem> StageNames { get; set; } = [
        new("Setup", true), new("Squadrons"), new("Pools"), new("Brackets"), new("Results")
        ];

    public Guid Guid { get; protected set; }

    public StageViewModel CurrentStage => Stages.Peek();

    protected Stack<StageViewModel> Stages { get; set; } = [];

    #endregion

    #region Commands

    [RelayCommand]
    private static void GoHome() => WeakReferenceMessenger.Default.Send<NavigateHomeMessage>();

    #endregion

    /// <summary>
    /// Creates a brand new tournament
    /// </summary>
    public TournamentViewModel()
    {
        Guid = Guid.NewGuid();
        Stages.Push(new SetupStageViewModel());
        SetupListeners();
    }

    /// <summary>
    /// Loads an existing tournament
    /// </summary>
    public TournamentViewModel(Tournament model)
    {
        Guid = model.Id;
        foreach (var stage in model.Stages)
            Stages.Push(stage.ToViewModel());
        SetupListeners();
    }

    /// <summary>
    /// Listens for messages
    /// </summary>
    private void SetupListeners()
    {
        WeakReferenceMessenger.Default.Register<TournamentViewModel, NextStageMessage>(this, (r, m) =>
        {
            Stages.Push(m.NextStage);
            OnPropertyChanged(nameof(CurrentStage));

            // activate the next stage
            for (int i = 0; i < StageNames.Count; i++)
            {
                if (StageNames[i].IsActive)
                {
                    StageNames[i++].IsActive = false;
                    if (i < StageNames.Count)
                        StageNames[i].IsActive = true;
                }
            }
        });
    }

    /// <summary>
    /// Converts to a <see cref="Tournament"/>
    /// </summary>
    public Tournament ToModel() => new(Guid, [.. Stages.Select(s => s.ToModel())]);
}
