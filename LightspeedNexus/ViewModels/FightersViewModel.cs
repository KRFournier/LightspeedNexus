using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class FightersViewModel : ViewModelBase, IComparer
{
    #region Properties

    public ObservableCollection<FighterViewModel> Fighters { get; set; } = [];

    public DataGridCollectionView SortedFighters { get; }

    [ObservableProperty]
    public partial FighterViewModel? Current { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;
    partial void OnSearchTextChanged(string value) => SortedFighters.Refresh();

    #endregion

    #region Commands

    [RelayCommand]
    private void ClearSearch() => SearchText = "";

    [RelayCommand]
    private static void GoHome() => WeakReferenceMessenger.Default.Send<NavigateHomeMessage>();

    [RelayCommand]
    private async Task NewFighter()
    {
        try
        {
            var result = await DialogBox(new FighterViewModel(), "New Fighter");
            if (result.IsOk)
            {
                Fighters.Add(result.Item);
                StorageService.Write(result.Item.ToModel());
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error creating and saving a new fighter: {e}");
        }
    }

    [RelayCommand]
    private async Task EditFighter(FighterViewModel item)
    {
        try
        {
            var result = await DialogBox(item.Clone(), "Edit Fighter", DialogButton.Delete);
            if (result.IsOk)
            {
                item.Update(result.Item);
                StorageService.Write(result.Item.ToModel());
                SortedFighters.Refresh();
            }
            else if (result.IsDeleted)
            {
                Fighters.Remove(item);
                StorageService.Write(result.Item.ToModel());
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error editing a fighter: {e}");
        }
    }

    [RelayCommand]
    private async Task ImportFighters()
    {
        (var success, var message, var fighters) = await SaberSportsService.GetAllFighters();
        if (success)
        {
            List<Fighter> fightersToAdd = [];

            // look up fighter and update, or add if doesn't exist
            foreach (var fighter in fighters)
            {
                var existing = Fighters.FirstOrDefault(f => f.OnlineId == fighter.OnlineId ||
                    (string.Compare(f.FirstName, fighter.FirstName, true) == 0 &&
                     string.Compare(f.LastName, fighter.LastName, true) == 0)
                    );
                if (existing is not null)
                {
                    existing.Club = string.IsNullOrEmpty(existing.Club) ? fighter.Club : existing.Club;
                    existing.ReyRank = fighter.Rey > existing.ReyRank ? fighter.Rey : existing.ReyRank;
                    existing.RenRank = fighter.Ren > existing.RenRank ? fighter.Ren : existing.RenRank;
                    existing.TanoRank = fighter.Tano > existing.TanoRank ? fighter.Tano : existing.TanoRank;
                }
                else
                {
                    StorageService.Write(fighter);
                    fightersToAdd.Add(fighter);
                }
            }

            // we add new fighters at the end so the above loop doesn't have to search new additions
            foreach (var fighter in fightersToAdd)
                Fighters.Add(new(fighter));
        }
        else
            MessageBox(message);
    }

    #endregion

    /// <summary>
    /// Creates a new Fighters view model that loads all known fighters from storage and sorts them.
    /// </summary>
    public FightersViewModel()
    {
        LoadFighters();
        SortedFighters = new(Fighters);
        SortedFighters.SortDescriptions.Add(new DataGridComparerSortDescription(this, ListSortDirection.Ascending));
        SortedFighters.Filter = item =>
        {
            if (item is FighterViewModel fvm)
            {
                return string.IsNullOrWhiteSpace(SearchText) ||
                       fvm.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       fvm.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       fvm.OnlineId?.ToString().Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                       fvm.Club?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true;
            }
            return false;
        };
    }

    /// <summary>
    /// Loads the collection of fighters from persistent storage and updates the view model.
    /// </summary>
    /// <remarks>This method does not perform any action when in design mode. If an error occurs while reading
    /// from storage, the error is logged for debugging purposes and the collection is not updated.</remarks>
    private void LoadFighters()
    {
        if (Design.IsDesignMode)
            return;

        try
        {
            Fighters = [.. StorageService.ReadAll<Fighter>().Select(f => new FighterViewModel(f))];
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error reading all fighters: {e}");
        }
    }

    /// <summary>
    /// Implements comparison for use in sorting the collection view.
    /// </summary>
    public int Compare(object? x, object? y)
    {
        if (x is FighterViewModel fvm1 && y is FighterViewModel fvm2)
            return fvm1.CompareTo(fvm2);
        else
            throw new ArgumentException("Both parameters must be of type FighterViewModel.");
    }
}
