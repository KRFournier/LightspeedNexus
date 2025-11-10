using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections.Generic;

namespace LightspeedNexus.ViewModels;

public partial class TournamentViewModel : ViewModelBase
{
    #region Properties

    /// <summary>
    /// The tournament's unique identifier
    /// </summary>
    public Guid Guid { get; protected set; }

    /// <summary>
    /// The initial stage of the tournament
    /// </summary>
    public SetupStageViewModel SetupStage { get; set; }

    /// <summary>
    /// The previously completed stages of the tournament
    /// </summary>
    public IEnumerable<StageViewModel> PreviousStages
    {
        get
        {
            StageViewModel stage = SetupStage;
            while (stage.Next is not null)
            {
                yield return stage;
                stage = stage.Next;
            }
        }
    }

    /// <summary>
    /// The current stage of the tournament
    /// </summary>
    public StageViewModel CurrentStage
    {
        get
        {
            StageViewModel stage = SetupStage;
            while (stage.Next is not null)
                stage = stage.Next;
            return stage;
        }
    }

    #endregion

    #region Commands

    /// <summary>
    /// Saves the tournamnet and returns to the main menu
    /// </summary>
    [RelayCommand]
    private void GoHome()
    {
        Save();
        WeakReferenceMessenger.Default.Send<NavigateHomeMessage>();
    }

    #endregion

    /// <summary>
    /// Creates a brand new tournament
    /// </summary>
    public TournamentViewModel()
    {
        Guid = Guid.NewGuid();
        SetupStage = new();
        SetupListeners();
    }

    /// <summary>
    /// Loads an existing tournament
    /// </summary>
    public TournamentViewModel(Tournament model)
    {
        Guid = model.Id;
        SetupStage = new(model.SetupStage);
        SetupListeners();
    }

    /// <summary>
    /// Listens for messages so we can notify changes to the current stage
    /// </summary>
    private void SetupListeners() =>
        WeakReferenceMessenger.Default.Register<TournamentViewModel, StageChangedMessage>(this, (r, m) =>
        {
            OnPropertyChanged(nameof(PreviousStages));
            OnPropertyChanged(nameof(CurrentStage));
            Save();
        });

    /// <summary>
    /// Converts to a <see cref="Tournament"/>
    /// </summary>
    public Tournament ToModel() => new(Guid, SetupStage.ToModel(), CurrentStage.IsTournamentCompleted);

    /// <summary>
    /// Saves the tournament
    /// </summary>
    public void Save()
    {
        if (SetupStage.CanBegin())
            StorageService.WriteTournament(ToModel());
    }
}
