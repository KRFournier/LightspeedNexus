using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.ComponentModel;

namespace LightspeedNexus.ViewModels;

public partial class PlayerViewModel : FighterViewModel
{
    #region Properties

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

    /// <summary>
    /// Gets the effective rank of the fighter based on their weapon of choice and overall skill.
    /// </summary>
    /// <remarks>If the weapon of choice is not the best weapon, the returned rank may be a combination of the
    /// best weapon's rank and the weapon of choice's rank to reflect the fighter's overall proficiency. An exception is
    /// thrown if the weapon of choice has a higher rank than the best weapon.</remarks>
    public Rank Rank
    {
        get
        {
            if (UseEffectiveRank)
            {
                // if the weapon of choice is the best weapon, return that rank
                var bestWeapon = GetBestWeapon();
                if (WeaponOfChoice == bestWeapon.Class)
                    return bestWeapon.Rank;
                else
                {
                    var rank = GetWeaponRank(WeaponOfChoice);
                    if (rank > bestWeapon.Rank)
                        throw new InvalidOperationException("Weapon of choice rank cannot be higher than best weapon rank");

                    // if the weapon of choice is the same rank as the best weapon, return that rank
                    else if (rank == bestWeapon.Rank)
                        return rank;
                    else
                    {
                        // otherwise, the weapon of choice is ranked less than the best weapon,
                        // so combine the two ranks to take into account the fighter's overall skill
                        return Rank.Combine(bestWeapon.Rank, GetWeaponRank(WeaponOfChoice));
                    }
                }
            }
            else
                return GetWeaponRank(WeaponOfChoice);
        }
    }

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
    public PlayerViewModel(Fighter fighter) : base(fighter) { }

    /// <summary>
    /// Converts to a <see cref="Player"/>
    /// </summary>
    public new Player ToModel() => new(
        Guid, OnlineId, FirstName, LastName, Club, ReyRank, RenRank, TanoRank,
        Card, Honor, ForceCalls, IsEjected, WeaponOfChoice
        );

    /// <summary>
    /// Converts the a <see cref="Fighter"/>
    /// </summary>
    public Fighter ToFighterModel() => base.ToModel();

    /// <summary>
    /// Overridden to notify that Rank has changed when any of the individual weapon ranks change.
    /// </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(ReyRank) ||
            e.PropertyName == nameof(RenRank) ||
            e.PropertyName == nameof(TanoRank))
        {
            OnPropertyChanged(nameof(Rank));
        }
    }

    /// <summary>
    /// Determines if the Player is disqualified either by card or ejection
    /// </summary>
    public bool IsDisqualified => Card == Card.Black || IsEjected;

    /// <summary>
    /// Gets or sets a value indicating whether effective rank calculations are used in operations that support ranking.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Rank))]
    public partial bool UseEffectiveRank { get; set; } = false;
}
