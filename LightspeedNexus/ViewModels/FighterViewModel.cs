using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LightspeedNexus.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

public partial class FighterViewModel : ViewModelBase, IComparable<FighterViewModel>
{
    #region Properties

    [ObservableProperty]
    public partial Guid Guid { get; protected set; } = Guid.NewGuid();

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
    public partial Rank ReyRank { get; set; } = Rank.U;

    [ObservableProperty]
    public partial Rank RenRank { get; set; } = Rank.U;

    [ObservableProperty]
    public partial Rank TanoRank { get; set; } = Rank.U;

    public string FullName => $"{LastName}, {FirstName}";

    #endregion

    #region Commands

    [RelayCommand]
    private void IncrementRey() => ReyRank++;

    [RelayCommand]
    private void DecrementRey() => ReyRank--;

    [RelayCommand]
    private void IncrementRen() => RenRank++;

    [RelayCommand]
    private void DecrementRen() => RenRank--;

    [RelayCommand]
    private void IncrementTano() => TanoRank++;

    [RelayCommand]
    private void DecrementTano() => TanoRank--;

    #endregion

    /// <summary>
    /// Creates a brand new fighter
    /// </summary>
    public FighterViewModel() { }

    /// <summary>
    /// Loads an existing fighter
    /// </summary>
    public FighterViewModel(Fighter fighter)
    {
        Guid = fighter.Id;
        OnlineId = fighter.OnlineId;
        FirstName = fighter.FirstName;
        LastName = fighter.LastName;
        Club = fighter.Club;
        ReyRank = fighter.Rey;
        RenRank = fighter.Ren;
        TanoRank = fighter.Tano;
    }

    /// <summary>
    /// Converts to a <see cref="Fighter"/>
    /// </summary>
    public Fighter ToModel() => new(Guid, OnlineId, FirstName, LastName, Club, ReyRank, RenRank, TanoRank);

    /// <summary>
    /// Creates a copy.
    /// </summary>
    public FighterViewModel Clone() => new(ToModel());

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
    /// Compares the current Fighter to another Fighter.
    /// </summary>
    /// <remarks>Compares last name, then first name, then OnlineId.</remarks>
    public int CompareTo(FighterViewModel? other)
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
    /// Updates this viewmodel with values from the given one. Useful for dialog boxes that edit fighter info.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the specified fighter does not have the same unique identifier as the current fighter.</exception>
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

}
