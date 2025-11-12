using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

#region Messages

/// <summary>
/// Requests the indicies of the given participants
/// </summary>
public class RequestParticipantIndicies(IEnumerable<ParticipantViewModel> participants) : RequestMessage<int[]>
{
    public IEnumerable<ParticipantViewModel> Participants { get; set; } = participants;
}

/// <summary>
/// Requests the index of the given participant
/// </summary>
public class RequestParticipantIndex(ParticipantViewModel participant) : RequestMessage<int>
{
    public ParticipantViewModel Participant { get; set; } = participant;
}

/// <summary>
/// Requests the indicies of the given squadrons
/// </summary>
public class RequestSquadronIndicies(IEnumerable<SquadronViewModel> squadrons) : RequestMessage<int[]>
{
    public IEnumerable<SquadronViewModel> Squadrons { get; set; } = squadrons;
}

/// <summary>
/// Requests the index of the given squadron
/// </summary>
public class RequestSquadronIndex(SquadronViewModel squadron) : RequestMessage<int>
{
    public SquadronViewModel Squadron { get; set; } = squadron;
}

#endregion

/// <summary>
/// The tournament settings
/// </summary>
public partial class SquadronsStageViewModel : StageViewModel,
    IRecipient<RequestParticipantIndicies>, IRecipient<RequestParticipantIndex>,
    IRecipient<RequestSquadronIndicies>, IRecipient<RequestSquadronIndex>
{
    #region Properties

    public ObservableCollection<ParticipantViewModel> Participants { get; set; } = [];

    [ObservableProperty]
    public partial bool IsAutoAssigned { get; set; } = true;
    partial void OnIsAutoAssignedChanged(bool value)
    {
        if (value)
            UpdateSquadrons();
    }

    public ObservableCollection<SquadronViewModel> Squadrons { get; set; } = [];

    /// <summary>
    /// Some names and colors we can use for squadrons
    /// </summary>
    static readonly (string Name, string Color)[] SquadronNames = [
        ("Red", "Red"),
        ("Blue", "Blue"),
        ("Green", "Green"),
        ("Purple", "Purple"),
        ("Yellow", "Yellow"),
        ("Light", "White"),
        ("Dark", "Gray"),
        ("Fire", "Orange"),
        ("Laser", "Lime"),
        ("Ice", "Teal"),
        ("Nova", "Violet"),
        ("Imperial", "Magenta"),
        ];

    /// <summary>
    /// The max a squadron size can be
    /// </summary>
    public static readonly int MaxSquadronSize = SquadronNames.Length;

    /// <summary>
    /// The minimum squadrons needed to ensure no squadron exceeds the max limit
    /// yet still accounts for all players in the roster
    /// </summary>
    public int MinSquadrons => (int)Math.Ceiling(Participants.Count / (double)MaxSquadronSize);

    /// <summary>
    /// The maximum size an automatic squadron can be
    /// </summary>
    const int AutoSquadronSize = 7;

    #endregion

    #region Message Handlers

    public void Receive(RequestParticipantIndicies message) =>
        message.Reply([.. message.Participants.Select(p => Participants.IndexOf(p))]);

    public void Receive(RequestParticipantIndex message) =>
        message.Reply(Participants.IndexOf(message.Participant));

    public void Receive(RequestSquadronIndicies message) =>
        message.Reply([.. message.Squadrons.Select(s => Squadrons.IndexOf(s))]);

    public void Receive(RequestSquadronIndex message) =>
        message.Reply(Squadrons.IndexOf(message.Squadron));

    #endregion

    /// <summary>
    /// Default constructor
    /// </summary>
    public SquadronsStageViewModel() : base("Squadrons")
    {
        StrongReferenceMessenger.Default.RegisterAll(this);
    }

    /// <summary>
    /// Creates brand new settings
    /// </summary>
    public SquadronsStageViewModel(IEnumerable<ParticipantViewModel> participants) : this()
    {
        IsAutoAssigned = true;
        foreach (var participant in participants)
            Participants.Add(participant);
        UpdateSquadrons();
    }

    /// <summary>
    /// Loads settings from a model
    /// </summary>
    public SquadronsStageViewModel(SquadronsStage model) : this()
    {
        IsAutoAssigned = model.IsAutoAssigned;
        Participants = [.. model.Participants.Select(p => ParticipantViewModel.FromModel(p))];

        int i = 0;
        Squadrons = [.. model.Squadrons.Select(s => new SquadronViewModel(s, Participants)
            {
                Name = SquadronNames[i].Name,
                Color = App.Current?.FindResource($"{SquadronNames[i++].Color}Brush") as IBrush ?? Brushes.Transparent
            })];
    }

    /// <summary>
    /// Converts into a model
    /// </summary>
    public override SquadronsStage ToModel() => new(
        IsAutoAssigned,
        [.. Participants.Select(p => p.ToModel())],
        [.. Squadrons.Select(s => s.ToModel())],
        Next?.ToModel());

    /// <summary>
    /// Go to the Pools Stage
    /// </summary>
    [RelayCommand]
    private void StartPools() => Next = new PoolsStageViewModel(Squadrons);

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
    private void Refresh() => UpdateSquadrons();

    /// <summary>
    /// Adds a new squadron
    /// </summary>
    [RelayCommand]
    private void AddSquadron()
    {
        if (Squadrons.Count < MaxSquadronSize)
        {
            IsAutoAssigned = false;
            CreateAndAddSquadron();
            UpdateSquadrons();
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
            RemoveSquadron(Squadrons.Count - 1);
            UpdateSquadrons();
        }
    }

    /// <summary>
    /// Creates a new squadron using the given index into SquadronNames
    /// </summary>
    private void CreateAndAddSquadron()
    {
        if (Squadrons.Count < MaxSquadronSize)
        {
            Squadrons.Add(new SquadronViewModel()
            {
                Name = SquadronNames[Squadrons.Count].Name,
                Color = App.Current?.FindResource($"{SquadronNames[Squadrons.Count].Color}Brush") as IBrush ?? Brushes.Transparent
            });
        }
    }

    /// <summary>
    /// Removes the squadron at the specified index from the collection.
    /// </summary>
    private void RemoveSquadron(int index)
    {
        if (index >= 0 && index < Squadrons.Count)
            Squadrons.RemoveAt(index);
    }

    /// <summary>
    /// Uses a bin packing algorithm to repopulate the pools in a balanced fashion
    /// </summary>
    private void UpdateSquadrons()
    {
        // auto calculate the squadrons, or base it on the requested number
        int new_count = IsAutoAssigned
            ? (int)Math.Ceiling(Participants.Count / (double)AutoSquadronSize)
            : Math.Max(Squadrons.Count, MinSquadrons);

        // remove extra squadrons
        while (Squadrons.Count > new_count)
            RemoveSquadron(Squadrons.Count - 1);

        // we shouldn't have more squadrons than we have names to give them
        if (new_count > SquadronNames.Length)
            throw new InvalidOperationException($"Cannot have more than {SquadronNames.Length} squadrons.");

        if (new_count > 0)
        {
            // add or remove squadrons
            for (int i = Squadrons.Count; i < new_count; i++)
                CreateAndAddSquadron();

            // reset members
            foreach (var s in Squadrons)
                s.Clear();

            // group participants by power level and assign from highest to lowest
            foreach (var group in Participants.GroupBy(p => p.PowerLevel).OrderByDescending(g => g.Key))
                RandomlyAssign([.. group]);

            // move last participant from squadrons that are 2+ larger than the smallest squadrons
            while (Squadrons.Max(s => s.Participants.Count) - Squadrons.Min(s => s.Participants.Count) > 1)
            {
                var small = Squadrons.MinBy(s => s.Participants.Count);
                if (small is not null)
                {
                    var big = Squadrons.MaxBy(s => s.Participants.Count);
                    if (big is not null && big.Participants.Count > 0)
                    {
                        int w = big.Participants[^1].PowerLevel;
                        small.Participants.Add(big.Participants[^1]);
                        small.Weight += w;
                        big.Weight -= w;
                        big.Participants.RemoveAt(big.Participants.Count - 1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Assigns given players to the smallest squadron. If multiple squadrons
    /// tie for the smallest, then the player is assigned randomly.
    /// </summary>
    private void RandomlyAssign(IList<ParticipantViewModel> participants)
    {
        Random r = new();

        // place each player into the squadron with the smallest total Value
        while (participants.Count > 0)
        {
            int i = r.Next(participants.Count);
            var participant = participants[i];
            int w = participant.PowerLevel;
            var squadron = Squadrons.MinBy(s => s.Weight);
            if (squadron is not null)
            {
                squadron.Participants.Add(participant);
                squadron.Weight += w;
            }
            participants.RemoveAt(i);
        }
    }

    #endregion
}
