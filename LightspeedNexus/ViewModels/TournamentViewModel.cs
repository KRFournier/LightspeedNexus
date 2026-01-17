using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LightspeedNexus.ViewModels;

#region Messages

public sealed class RequestIsRankedMessage : RequestMessage<bool?> { }

#endregion

public partial class TournamentViewModel : ViewModelBase, IDisposable,
    IRecipient<NextStageMessage>, IRecipient<PreviousStageMessage>,
    IRecipient<RosterChangedMessage>, IRecipient<BracketRoundCompleted>,
    IRecipient<RequestIsRankedMessage>
{
    #region Properties

    /// <summary>
    /// The tournament's unique identifier
    /// </summary>
    public Guid Guid { get; protected set; } = Guid.NewGuid();

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

    /// <summary>
    /// The previous stage of the tournament
    /// </summary>
    public StageViewModel? PreviousStage => CurrentStage.Previous;

    /// <summary>
    /// Finds the stage of the given type in the tournament, or null if it doesn't exist
    /// </summary>
    public T? FindStage<T>() where T : StageViewModel
    {
        StageViewModel? stage = SetupStage;
        while (stage is not null)
        {
            if (stage is T t)
                return t;
            stage = stage.Next;
        }
        return null;
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

    #region Message Handlers

    public void Receive(NextStageMessage message)
    {
        OnPropertyChanged(nameof(PreviousStages));
        OnPropertyChanged(nameof(CurrentStage));
        Save();
    }

    public void Receive(PreviousStageMessage message)
    {
        message.CurrentStage?.Dispose();
        message.PreviousStage?.Next = null;
        OnPropertyChanged(nameof(PreviousStages));
        OnPropertyChanged(nameof(CurrentStage));
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(InitialRank));
        OnPropertyChanged(nameof(FinalRank));
        Save();
    }

    public void Receive(RosterChangedMessage message)
    {
        OnPropertyChanged(nameof(IsRanked));
        OnPropertyChanged(nameof(InitialRank));
        OnPropertyChanged(nameof(Value));
    }

    public void Receive(BracketRoundCompleted message)
    {
        OnPropertyChanged(nameof(FinalRank));
    }

    public void Receive(RequestIsRankedMessage message)
    {
        message.Reply(IsRanked);
    }

    #endregion

    private bool _loading = true;

    /// <summary>
    /// Creates a brand new tournament
    /// </summary>
    public TournamentViewModel()
    {
        SetupStage = new();
        StrongReferenceMessenger.Default.RegisterAll(this);
        _loading = false;
    }

    /// <summary>
    /// Loads an existing tournament
    /// </summary>
    public TournamentViewModel(Tournament model)
    {
        Guid = model.Id;
        StrongReferenceMessenger.Default.RegisterAll(this);
        SetupStage = SetupStageViewModel.FromModel(model.SetupStage);

        // have to set this manually since the stages are loaded in bulk and won't trigger the property changed events
        FindStage<BracketStageViewModel>()?.IsRanked = IsRanked;
        _loading = false;
    }

    /// <summary>
    /// Converts to a <see cref="Tournament"/>
    /// </summary>
    public Tournament ToModel() => new()
    {
        Id = Guid,
        SetupStage = SetupStage.ToModel(),
        IsCompleted = CurrentStage.IsTournamentCompleted
    };

    /// <summary>
    /// Saves the tournament
    /// </summary>
    public void Save()
    {
        if (!_loading && SetupStage.CanBegin())
        {
            StorageService.WriteTournament(ToModel());
            SetupStage.OnTournamentSaved();
        }
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

    [ObservableProperty]
    public partial string? TopRanked { get; protected set; }

    /// <summary>
    /// The tournament's initial rank
    /// </summary>
    public string? InitialRank
    {
        get
        {
            if (!IsRanked)
                return "Unranked";

            var grading = GradingsChart.FindInitial(SetupStage.Registrees.Select(r => r.Rank));
            if (grading is not null)
            {
                TopRanked = $"Top {grading.TopX}";
                return grading.Rating;
            }

            return null;
        }
    }

    /// <summary>
    /// The tournament's initial rank
    /// </summary>
    public string? FinalRank
    {
        get
        {
            if (!IsRanked)
                return "Unranked";

            int topX = GradingsChart.GetTopX(SetupStage.Registrees.Count);
            if (topX > 0)
            {
                var topPlayers = FindStage<BracketStageViewModel>()?.GetTopXParticipants(topX);
                if (topPlayers is not null)
                {
                    var grading = GradingsChart.FindFinal(
                        SetupStage.Registrees.Select(r => r.Rank),
                        topPlayers.OfType<PlayerViewModel>().Select(p => p.Rank)
                        );

                    if (grading is not null)
                        return grading.Rating;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Calculates the value of the tournament based on the registrees' ranks
    /// </summary>
    public int Value => SetupStage.Registrees.Select(r => r.Rank.Power).Sum();

    #endregion
}
