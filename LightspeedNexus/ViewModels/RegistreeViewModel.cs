using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;

namespace LightspeedNexus.ViewModels;

public partial class RegistreeViewModel : ViewModelBase, IComparable<RegistreeViewModel>    
{
    #region Properties

    [ObservableProperty]
    public partial Guid Guid { get; protected set; } = Guid.Empty;

    [ObservableProperty]
    public partial int? OnlineId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    public partial string LastName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? Club { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Rank))]
    public partial Rank ReyRank { get; set; } = Rank.U;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Rank))]
    public partial Rank RenRank { get; set; } = Rank.U;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Rank))]
    public partial Rank TanoRank { get; set; } = Rank.U;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Rank))]
    public partial bool UseEffectiveRank { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Rank))]
    public partial WeaponClass WeaponOfChoice { get; set; } = WeaponClass.Rey;

    [ObservableProperty]
    public partial bool MeetsRequirements { get; set; } = true;

    public string FullName => $"{LastName}, {FirstName}";

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

    #endregion

    /// <summary>
    /// Converts to a <see cref="Player"/>
    /// </summary>
    public Registree ToModel() => new()
    {
        Id = Guid,
        OnlineId = OnlineId,
        FirstName = FirstName,
        LastName = LastName,
        Club = Club,
        Rey = ReyRank,
        Ren = RenRank,
        Tano = TanoRank,
        UsesEffectiveRank = UseEffectiveRank,
        WeaponOfChoice = WeaponOfChoice
    };

    /// <summary>
    /// Loads an existing Player
    /// </summary>
    public static RegistreeViewModel FromModel(Registree registree) => new()
    {
        Guid = registree.Id,
        OnlineId = registree.OnlineId,
        FirstName = registree.FirstName,
        LastName = registree.LastName,
        Club = registree.Club,
        ReyRank = registree.Rey,
        RenRank = registree.Ren,
        TanoRank = registree.Tano,
        UseEffectiveRank = registree.UsesEffectiveRank,
        WeaponOfChoice = registree.WeaponOfChoice
    };

    /// <summary>
    /// Converts to a <see cref="FighterViewModel"/>
    /// </summary>
    public FighterViewModel ToFighterViewModel() => new()
    {
        Guid = Guid,
        OnlineId = OnlineId,
        FirstName = FirstName,
        LastName = LastName,
        Club = Club,
        ReyRank = ReyRank,
        RenRank = RenRank,
        TanoRank = TanoRank
    };

    /// <summary>
    /// Updates this registree with values from the given fighter. Useful for dialog boxes that edit fighter info.
    /// </summary>
    public void Update(FighterViewModel fighter)
    {
        if (fighter.Guid != Guid)
            throw new InvalidOperationException("Cannot update fighter info with a different fighter");
        OnlineId = fighter.OnlineId;
        FirstName = fighter.FirstName;
        LastName = fighter.LastName;
        Club = fighter.Club;
        ReyRank = fighter.ReyRank;
        RenRank = fighter.RenRank;
        TanoRank = fighter.TanoRank;
    }

    /// <summary>
    /// Compares the current Fighter to another Fighter.
    /// </summary>
    /// <remarks>Compares last name, then first name, then OnlineId.</remarks>
    public int CompareTo(RegistreeViewModel? other)
    {
        if (other is null)
            return 1;

        var lastNameComparison = string.Compare(LastName, other.LastName, StringComparison.OrdinalIgnoreCase);
        if (lastNameComparison != 0)
            return lastNameComparison;
        var firstNameComparison = string.Compare(FirstName, other.FirstName, StringComparison.OrdinalIgnoreCase);
        if (firstNameComparison != 0)
            return firstNameComparison;
        return (OnlineId ?? int.MinValue) - (other.OnlineId ?? int.MinValue);
    }

    /// <summary>
    /// Returns the <see cref="WeaponRating"/> with the highest rank among all available weapon classes.
    /// </summary>
    public WeaponRating GetBestWeapon() =>
        new WeaponRating[] {
            new(WeaponClass.Rey, ReyRank),
            new(WeaponClass.Ren, RenRank),
            new(WeaponClass.Tano, TanoRank)
        }.OrderByDescending(w => w.Rank).First();

    /// <summary>
    /// Gets the rank associated with the specified weapon class.
    /// </summary>
    /// <param name="weaponClass">The weapon class for which to retrieve the rank.</param>
    /// <returns>The rank corresponding to the specified weapon class. Returns <see cref="Rank.U"/> if the weapon class is not
    /// recognized.</returns>
    public Rank GetWeaponRank(WeaponClass weaponClass) => weaponClass switch
    {
        WeaponClass.Rey => ReyRank,
        WeaponClass.Ren => RenRank,
        WeaponClass.Tano => TanoRank,
        _ => Rank.U
    };

    /// <summary>
    /// Determines if the current weapon meets the given requirements
    /// </summary>
    public void Validate(bool allowARanks, bool allowBRanks, bool allowCRanks, bool allowDRanks, bool allowERanks, bool allowURanks)
    {
        MeetsRequirements = ValidateWeapon(WeaponOfChoice, allowARanks, allowBRanks, allowCRanks, allowDRanks, allowERanks, allowURanks);
    }

    /// <summary>
    /// Determines if the current weapon meets the given requirements
    /// </summary>
    private bool ValidateWeapon(WeaponClass weaponClass, bool allowARanks, bool allowBRanks, bool allowCRanks, bool allowDRanks, bool allowERanks, bool allowURanks)
        => GetWeaponRank(weaponClass).Letter switch
        {
            'A' => allowARanks,
            'B' => allowBRanks,
            'C' => allowCRanks,
            'D' => allowDRanks,
            'E' => allowERanks,
            'U' => allowURanks,
            _ => false
        };
}
