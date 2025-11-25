using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;

namespace LightspeedNexus.ViewModels;

public abstract partial class ParticipantViewModel : ViewModelBase
{
    #region Properties

    /// <summary>
    /// The participant's name
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>
    /// The participant's power level, based on rank or team members' ranks
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Power))]
    public partial int PowerLevel { get; set; } = 0;

    /// <summary>
    /// The string representation of a participant's power. It defaults to just the
    /// power level number, but it could be overriden to show, for example, rank.
    /// </summary>
    public virtual string Power => PowerLevel.ToString();

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
        PowerLevel = participant.PowerLevel;
    }

    public abstract Participant ToModel();

    public static ParticipantViewModel FromModel(Participant participant) => participant switch
    {
        Player player => new PlayerViewModel(player),
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
    public override string Power => Rank.ToString();

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
        OnlineId = Player.OnlineId;
        Club = Player.Club;
        Rank = Player.Rank;
        Card = Player.Card;
        Honor = Player.Honor;
        ForceCalls = Player.ForceCalls;
        IsEjected = Player.IsEjected;
        WeaponOfChoice = Player.WeaponOfChoice;
    }

    /// <summary>
    /// Creates a new Player from fighter info
    /// </summary>
    public PlayerViewModel(RegistreeViewModel registree, string name) : base()
    {
        Name = name;
        WeaponOfChoice = registree.WeaponOfChoice;
        Rank = registree.Rank;
    }

    /// <summary>
    /// Converts to a <see cref="Player"/>
    /// </summary>
    public override Player ToModel() => new(Name, PowerLevel,
        OnlineId, Club, Rank, Card, Honor, ForceCalls, IsEjected, WeaponOfChoice);

    /// <summary>
    /// Determines if the Player is disqualified either by card or ejection
    /// </summary>
    public override bool IsDisqualified => Card == Card.Black || IsEjected;
}
