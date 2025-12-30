using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;

namespace LightspeedNexus.ViewModels;

public abstract partial class ParticipantViewModel : ViewModelBase
{
    #region Properties

    /// <summary>
    /// The participant's unique identifier
    /// </summary>
    [ObservableProperty]
    public partial Guid Guid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The participant's name
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>
    /// The participant's power level, based on rank or team members' ranks
    /// </summary>
    [ObservableProperty]
    public partial int PowerLevel { get; set; } = 0;

    /// <summary>
    /// Used by parent view models to set dragging state
    /// </summary>
    [ObservableProperty]
    public partial bool IsDragging { get; set; } = false;

    /// <summary>
    /// Indiciates whether or not this participant is disqualified
    /// </summary>
    public abstract bool IsDisqualified { get; }

    #endregion

    public abstract Participant ToModel();

    public static ParticipantViewModel FromModel(Participant participant) => participant switch
    {
        Player player => PlayerViewModel.FromModel(player),
        _ => throw new NotSupportedException($"Participant type {participant.GetType().Name} is not supported."),
    };
}

/// <summary>
/// A placeholder to indicate that a participant has a bye
/// </summary>
public sealed partial class ByeViewModel : ParticipantViewModel
{
    public override Bye ToModel() => new();
    public override bool IsDisqualified => true;
}

/// <summary>
/// A player
/// </summary>
public sealed partial class PlayerViewModel : ParticipantViewModel
{
    #region Properties

    [ObservableProperty]
    public partial int? OnlineId { get; set; }

    [ObservableProperty]
    public partial string? Club { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDisqualified))]
    public partial Card Card { get; set; } = Card.None;

    [ObservableProperty]
    public partial int Honor { get; set; } = 0;

    [ObservableProperty]
    public partial int ForceCalls { get; set; } = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDisqualified))]
    public partial bool IsEjected { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Rank))]
    public partial WeaponClass WeaponOfChoice { get; set; } = WeaponClass.Rey;

    [ObservableProperty]
    public partial Rank Rank { get; set; } = Rank.U;
    partial void OnRankChanged(Rank value) => PowerLevel = Rank.Weight;

    #endregion

    /// <summary>
    /// Converts to a <see cref="Player"/>
    /// </summary>
    public override Player ToModel() => new()
    {
        Id = Guid,
        Name = Name,
        PowerLevel = PowerLevel,
        OnlineId = OnlineId,
        Club = Club,
        Rank = Rank,
        Card = Card,
        Honor = Honor,
        ForceCalls = ForceCalls,
        IsEjected = IsEjected,
        WeaponOfChoice = WeaponOfChoice
    };

    /// <summary>
    /// Creates a new view model from a <see cref="Player"/>
    /// </summary>
    public static PlayerViewModel FromModel(Player player) => new()
    {
        Guid = player.Id,
        Name = player.Name,
        PowerLevel = player.PowerLevel,
        OnlineId = player.OnlineId,
        Club = player.Club,
        Rank = player.Rank,
        Card = player.Card,
        Honor = player.Honor,
        ForceCalls = player.ForceCalls,
        IsEjected = player.IsEjected,
        WeaponOfChoice = player.WeaponOfChoice
    };

    /// <summary>
    /// Creates a player view model from a registree and a specified name
    /// </summary>
    public static PlayerViewModel FromRegistree(RegistreeViewModel registree, string name) => new()
    {
        Name = name,
        OnlineId = registree.OnlineId,
        Club = registree.Club,
        Rank = registree.Rank,
        WeaponOfChoice = registree.WeaponOfChoice
    };

    /// <summary>
    /// Determines if the Player is disqualified either by card or ejection
    /// </summary>
    public override bool IsDisqualified => Card == Card.Black || IsEjected;
}
