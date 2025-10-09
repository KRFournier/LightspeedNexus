using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.ComponentModel;
using System.Numerics;

namespace LightspeedNexus.ViewModels;

public partial class PlayerViewModel : FighterViewModel
{
    #region Properties

    [ObservableProperty]
    public partial Card Card { get; set; } = Card.None;

    [ObservableProperty]
    public partial int Honor { get; set; } = 0;

    [ObservableProperty]
    public partial int ForceCalls { get; set; } = 0;

    [ObservableProperty]
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
    }

    /// <summary>
    /// Used by parent view models to set dragging state
    /// </summary>
    [ObservableProperty]
    public partial bool IsDragging { get; set; } = false;

    #endregion

    /// <summary>
    /// Creates a brand new player
    /// </summary>
    public PlayerViewModel() : base() { }

    /// <summary>
    /// Loads an existing player
    /// </summary>
    public PlayerViewModel(Player player) : base(player)
    {
        Card = player.Card;
        Honor = player.Honor;
        ForceCalls = player.ForceCalls;
        IsEjected = player.IsEjected;
        WeaponOfChoice = player.WeaponOfChoice;
    }

    /// <summary>
    /// Creates a new player from fighter info
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
}
