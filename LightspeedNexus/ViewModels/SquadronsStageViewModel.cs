using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using LightspeedNexus.Transitions;
using System.Collections.ObjectModel;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// The tournament settings
/// </summary>
public partial class SquadronsStageViewModel(IServiceProvider serviceProvider, IMessenger messenger, NavigationService navigationService, SquadronService squadronService)
    : StageViewModel(serviceProvider, messenger, navigationService)
{
    #region Properties

    public override string Name => "Squadrons";

    public ObservableCollection<ParticipantViewModel> Participants { get; set; } = [];

    [ObservableProperty]
    public partial bool IsAutoAssigned { get; set; } = true;
    partial void OnIsAutoAssignedChanged(bool value)
    {
        if (value)
            squadronService.RebalanceSquadrons(this);
    }

    public ObservableCollection<SquadronViewModel> Squadrons { get; set; } = [];

    /// <summary>
    /// The max a squadron size can be
    /// </summary>
    public readonly int MaxSquadronSize = squadronService.MaxSquadrons;

    /// <summary>
    /// The minimum squadrons needed to ensure no squadron exceeds the max limit
    /// yet still accounts for all players in the roster
    /// </summary>
    public int MinSquadrons => (int)Math.Ceiling(Participants.Count / (double)MaxSquadronSize);

    /// <summary>
    /// The maximum size an automatic squadron can be
    /// </summary>
    public int AutoSquadronSize => SquadronService.AutoSquadronSize;

    #endregion

    /// <summary>
    /// Converts into a model
    /// </summary>
    public override SquadronsStage ToModel() => new()
    {
        IsAutoAssigned = IsAutoAssigned,
        Participants = [.. Participants.Select(p => p.ToModel())],
        Squadrons = [.. Squadrons.Select(s => s.ToModel())],
        Next = Next?.ToModel()
    };

    /// <summary>
    /// Squadrons always go to pools
    /// </summary>
    public override IStageTransition GetTransitionToNextStage() => New<SquadronsToPoolsTransition>();

    #region Drag and Drop

    /// <summary>
    /// The participant that is being dragged
    /// </summary>
    [ObservableProperty]
    public partial ParticipantViewModel? DraggingParticipant { get; set; }
    partial void OnDraggingParticipantChanged(ParticipantViewModel? oldValue, ParticipantViewModel? newValue)
    {
        if (oldValue is not null) oldValue.IsDragging = false;
        if (newValue is not null) newValue.IsDragging = true;
    }

    /// <summary>
    /// As the player drags, we drop the dragging player onto the current one
    /// </summary>
    public void DropOnPlayer(ParticipantViewModel target)
    {
        if (DraggingParticipant is null || DraggingParticipant == target)
            return;

        int iDrag = -1;
        foreach (var squad in Squadrons)
        {
            iDrag = squad.Participants.IndexOf(DraggingParticipant);
            if (iDrag >= 0)
            {
                int iTarget = squad.Participants.IndexOf(target);
                if (iTarget >= 0)
                {
                    (squad.Participants[iDrag], squad.Participants[iTarget]) = (squad.Participants[iTarget], squad.Participants[iDrag]);
                    return;
                }
                else
                {
                    squad.Participants.RemoveAt(iDrag);
                    squad.Weight -= DraggingParticipant.PowerLevel;
                    break;
                }
            }
        }

        // add it before
        if (iDrag >= 0)
        {
            foreach (var squad in Squadrons)
            {
                int i = squad.Participants.IndexOf(target);
                if (i >= 0)
                {
                    squad.Participants.Insert(i, DraggingParticipant);
                    squad.Weight += DraggingParticipant.PowerLevel;
                    return;
                }
            }
        }
    }

    public void DropOnSquadron(SquadronViewModel targetSquadron)
    {
        if (DraggingParticipant is null)
            return;
        int iDrag = -1;
        foreach (var squad in Squadrons)
        {
            iDrag = squad.Participants.IndexOf(DraggingParticipant);
            if (iDrag >= 0)
            {
                squad.Participants.RemoveAt(iDrag);
                squad.Weight -= DraggingParticipant.PowerLevel;
                break;
            }
        }
        // add to target squadron
        if (iDrag >= 0)
        {
            targetSquadron.Participants.Add(DraggingParticipant);
            targetSquadron.Weight += DraggingParticipant.PowerLevel;
        }
    }

    #endregion

    #region Squadron Management

    /// <summary>
    /// This command is used to refresh the squadrons
    /// </summary>
    [RelayCommand]
    public void Refresh() => squadronService.RebalanceSquadrons(this);

    /// <summary>
    /// Adds a new squadron
    /// </summary>
    [RelayCommand]
    private void AddSquadron()
    {
        if (Squadrons.Count < MaxSquadronSize)
        {
            IsAutoAssigned = false;
            if (Squadrons.Count < MaxSquadronSize)
                Squadrons.Add(squadronService.GenerateSquadron(Squadrons.Count));
            squadronService.RebalanceSquadrons(this);
        }
    }

    /// <summary>
    /// Removes the last squadron
    /// </summary>
    [RelayCommand]
    private void RemoveSquadron()
    {
        if (Squadrons.Count > MinSquadrons)
        {
            if (IsAutoAssigned)
                IsAutoAssigned = false;
            Squadrons.RemoveAt(Squadrons.Count - 1);
            squadronService.RebalanceSquadrons(this);
        }
    }

    #endregion
}
