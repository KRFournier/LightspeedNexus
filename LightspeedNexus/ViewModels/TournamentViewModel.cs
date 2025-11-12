using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightspeedNexus.ViewModels;

public partial class TournamentViewModel : ViewModelBase, IDisposable,
    IRecipient<NextStageMessage>, IRecipient<PreviousStageMessage>,
    IRecipient<RosterChangedMessage>
{
    #region Properties

    /// <summary>
    /// The tournament's unique identifier
    /// </summary>
    public Guid Guid { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// The initial stage of the tournament
    /// </summary>
    public SetupStageViewModel SetupStage { get; set; } = new();

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

    /// <summary>
    /// The previous stage of the tournament
    /// </summary>
    public StageViewModel? PreviousStage => CurrentStage.Previous;

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

    #region Message Handlers

    public void Receive(NextStageMessage message)
    {
        OnPropertyChanged(nameof(PreviousStages));
        OnPropertyChanged(nameof(CurrentStage));
        Save();
    }

    public void Receive(PreviousStageMessage message)
    {
        message.PreviousStage?.Dispose();
        message.PreviousStage?.Next = null;
        OnPropertyChanged(nameof(PreviousStages));
        OnPropertyChanged(nameof(CurrentStage));
        Save();
    }

    public void Receive(RosterChangedMessage message)
    {
        OnPropertyChanged(nameof(IsRanked));
        OnPropertyChanged(nameof(InitialRank));
        OnPropertyChanged(nameof(Value));
    }

    #endregion

    /// <summary>
    /// Creates a brand new tournament
    /// </summary>
    public TournamentViewModel()
    {
        StrongReferenceMessenger.Default.RegisterAll(this);
    }

    /// <summary>
    /// Loads an existing tournament
    /// </summary>
    public TournamentViewModel(Tournament model) : this()
    {
        Guid = model.Id;
        SetupStage = new(model.SetupStage);
    }

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

    /// <summary>
    /// Cleans up messenger registrations
    /// </summary>
    public void Dispose()
    {
        StrongReferenceMessenger.Default.UnregisterAll(this);
        SetupStage.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Value and Rank

        /// <summary>
        /// Determines if the tournament is ranked
        /// </summary>
    public bool IsRanked => SetupStage.GameMode == GameMode.Standard && GradingsChart.IsRankable(SetupStage.Registrees.Count);

    /// <summary>
    /// The tournament's initial rank
    /// </summary>
    public string InitialRank
    {
        get
        {
            if (!IsRanked)
                return "Unranked";
            return GradingsChart.FindInitial(SetupStage.Registrees.Select(r => r.Rank))?.Rating ?? "Unranked";
        }
    }

    /// <summary>
    /// Calculates the value of the tournament based on the registrees' ranks
    /// </summary>
    public int Value => SetupStage.Registrees.Select(r => r.Rank.Power).Sum();

    #endregion
}
