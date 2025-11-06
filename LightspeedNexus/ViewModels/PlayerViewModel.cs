using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.ComponentModel;

namespace LightspeedNexus.ViewModels;

public abstract partial class ParticipantViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    #endregion

    /// <summary>
    /// Creates a brand new Participant
    /// </summary>
    public ParticipantViewModel() : base() { }

    /// <summary>
    /// Loads an existing Participant
    /// </summary>
    public ParticipantViewModel(Participant participant) : base()
    {
        Name = participant.Name;
    }
}

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

    /// <summary>
    /// Used by parent view models to set dragging state
    /// </summary>
    [ObservableProperty]
    public partial bool IsDragging { get; set; } = false;

    #endregion

    /// <summary>
    /// Creates a brand new Player
    /// </summary>
    public PlayerViewModel() : base() { }

    /// <summary>
    /// Loads an existing Player
    /// </summary>
    public PlayerViewModel(Player Player) : base(Player)
    {
        Card = Player.Card;
        Honor = Player.Honor;
        ForceCalls = Player.ForceCalls;
        IsEjected = Player.IsEjected;
        WeaponOfChoice = Player.WeaponOfChoice;
    }

    /// <summary>
    /// Creates a new Player from fighter info
    /// </summary>
    public PlayerViewModel(RegistreeViewModel registree) : base()    
    {
        Name = registree.FullName;
        WeaponOfChoice = registree.WeaponOfChoice;
        Rank = registree.Rank;
    }

    /// <summary>
    /// Converts to a <see cref="Player"/>
    /// </summary>
    public Player ToModel() => new(Name, OnlineId, Club, Rank, Card, Honor, ForceCalls, IsEjected, WeaponOfChoice);

    /// <summary>
    /// Determines if the Player is disqualified either by card or ejection
    /// </summary>
    public bool IsDisqualified => Card == Card.Black || IsEjected;
}
