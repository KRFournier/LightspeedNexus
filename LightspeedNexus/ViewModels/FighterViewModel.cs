using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.ViewModels;
using LightspeedNexus.Services;

namespace LightspeedNexus.ViewModels;

public partial class FighterViewModel(IServiceProvider serviceProvider, IMessenger messenger, LoadingService loadingService) : ViewModelBase(serviceProvider, messenger), IComparable<FighterViewModel>
{
    #region Properties

    [ObservableProperty]
    public partial Guid Guid { get; set; } = Guid.NewGuid();

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
    /// Converts to a <see cref="Fighter"/>
    /// </summary>
    public Fighter ToModel() => new()
    {
        Id = Guid,
        OnlineId = OnlineId,
        FirstName = FirstName,
        LastName = LastName,
        Club = Club,
        Rey = ReyRank,
        Ren = RenRank,
        Tano = TanoRank
    };

    /// <summary>
    /// Creates a copy.
    /// </summary>
    public FighterViewModel Clone()
    {
        var vm = loadingService.LoadFighter(ToModel());
        vm.Guid = Guid;
        vm.OnlineId = OnlineId;
        vm.FirstName = FirstName;
        vm.LastName = LastName;
        vm.Club = Club;
        vm.ReyRank = ReyRank;
        vm.RenRank = RenRank;
        vm.TanoRank = TanoRank;
        return vm;
    }

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

    #region Import Update Data

    [ObservableProperty]
    public partial bool IsNew { get; set; } = false;

    [ObservableProperty]
    public partial bool HasNewClub { get; set; } = false;

    [ObservableProperty]
    public partial bool HasNewReyRank { get; set; } = false;

    [ObservableProperty]
    public partial bool HasNewRenRank { get; set; } = false;

    [ObservableProperty]
    public partial bool HasNewTanoRank { get; set; } = false;

    public bool HasNew => IsNew;

    public void UpdateFromImported(Fighter fighter)
    {
        if (string.IsNullOrEmpty(Club))
        {
            Club = fighter.Club;
            HasNewClub = true;
        }

        if (fighter.Rey > ReyRank)
        {
            ReyRank = fighter.Rey;
            HasNewReyRank = true;
        }

        if (fighter.Ren > RenRank)
        {
            RenRank = fighter.Ren;
            HasNewRenRank = true;
        }

        if (fighter.Tano > TanoRank)
        {
            TanoRank = fighter.Tano;
            HasNewTanoRank = true;
        }
    }

    public FighterViewModel NewFromImported(Fighter fighter)
    {
        var vm = loadingService.LoadFighter(fighter);
        vm.IsNew = true;
        vm.HasNewClub = true;
        vm.HasNewReyRank = true;
        vm.HasNewRenRank = true;
        vm.HasNewTanoRank = true;
        return vm;
    }

    #endregion
}
