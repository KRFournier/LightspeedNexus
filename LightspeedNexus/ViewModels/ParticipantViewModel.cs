using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Models;
using System;
using System.ComponentModel;

namespace LightspeedNexus.ViewModels;

#region Messages

public sealed record ParticipantDisqualifedChanged(ParticipantViewModel Participant);

#endregion

public abstract partial class ParticipantViewModel : ViewModelBase
{
    #region Properties

    public abstract Guid Guid { get; }
    public abstract string Name { get; }
    public abstract int PowerLevel { get; }
    public abstract bool IsDisqualified { get; }
    public abstract bool IsBye { get; }
    public abstract bool IsEmpty { get; }

    /// <summary>
    /// Used by parent view models to set dragging state
    /// </summary>
    [ObservableProperty]
    public partial bool IsDragging { get; set; } = false;

    /// <summary>
    /// Used for Byes
    /// </summary>
    public static readonly ParticipantViewModel Bye = new ByeViewModel();

    /// <summary>
    /// Placeholder participant
    /// </summary>
    public static readonly ParticipantViewModel Empty = new EmptyParticipantViewModel();

    #endregion

    public abstract IParticipant ToModel();

    public static ParticipantViewModel FromModel(IParticipant participant) => participant switch
    {
        Player player => PlayerViewModel.FromModel(player),
        ByeParticipant => Bye,
        EmptyParticipant => Empty,
        _ => throw new NotSupportedException($"Participant type {participant.GetType().Name} is not supported."),
    };

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        // we need to let others know when a participant's disqualification status changes so that matches can update accordingly
        if (e.PropertyName == nameof(IsDisqualified))
            WeakReferenceMessenger.Default.Send(new ParticipantDisqualifedChanged(this), Guid);
        base.OnPropertyChanged(e);
    }

    public override string ToString() => Name;
}

/// <summary>
/// A placeholder for matches in which a participant has a bye
/// </summary>
public sealed partial class ByeViewModel : ParticipantViewModel
{
    public override Guid Guid => ByeParticipant.ByeGuid;
    public override string Name => string.Empty;
    public override int PowerLevel => 0;
    public override ByeParticipant ToModel() => new();
    public override bool IsBye => true;
    public override bool IsEmpty => false;
    public override bool IsDisqualified => true;
    public override string ToString() => "BYE";
}

/// <summary>
/// A placeholder for matches in which a participant has a bye
/// </summary>
public sealed partial class EmptyParticipantViewModel : ParticipantViewModel
{
    public override Guid Guid => Guid.Empty;
    public override string Name => string.Empty;
    public override int PowerLevel => 0;
    public override EmptyParticipant ToModel() => new();
    public override bool IsBye => false;
    public override bool IsEmpty => true;
    public override bool IsDisqualified => false;
    public override string ToString() => "EMPTY";
}

/// <summary>
/// A player
/// </summary>
public sealed partial class PlayerViewModel : ParticipantViewModel
{
    #region Abstract Implementation

    private Guid _guid = Guid.NewGuid();
    public override Guid Guid => _guid;

    private string _name = string.Empty;
    public override string Name => _name;

    public override int PowerLevel => Rank.Weight;

    public override bool IsBye => false;
    public override bool IsEmpty => false;

    #endregion

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
    [NotifyPropertyChangedFor(nameof(IsRey))]
    [NotifyPropertyChangedFor(nameof(IsRen))]
    [NotifyPropertyChangedFor(nameof(IsTano))]
    public partial WeaponClass WeaponOfChoice { get; set; } = WeaponClass.Rey;
    public bool IsRey => WeaponOfChoice == WeaponClass.Rey;
    public bool IsRen => WeaponOfChoice == WeaponClass.Ren;
    public bool IsTano => WeaponOfChoice == WeaponClass.Tano;

    [ObservableProperty]
    public partial bool ShowWeapons { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PowerLevel))]
    public partial Rank Rank { get; set; } = Rank.U;

    /// <summary>
    /// Determines if the Player is disqualified either by card or ejection
    /// </summary>
    public override bool IsDisqualified => Card == Card.Black || IsEjected;

    #endregion

    public PlayerViewModel()
    {
        if (Design.IsDesignMode)
            ShowWeapons = true;
        else
            ShowWeapons = StrongReferenceMessenger.Default.Send(new RequestHasChoice());
    }

    public PlayerViewModel(string name) : this()
    {
        _name = name;
    }

    /// <summary>
    /// Converts to a <see cref="Player"/>
    /// </summary>
    public override Player ToModel() => new()
    {
        Id = Guid,
        Name = Name,
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
        _guid = player.Id,
        _name = player.Name,
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
        _name = name,
        OnlineId = registree.OnlineId,
        Club = registree.Club,
        Rank = registree.Rank,
        WeaponOfChoice = registree.WeaponOfChoice
    };
}
