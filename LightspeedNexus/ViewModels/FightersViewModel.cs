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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class FightersViewModel : ViewModelBase, IComparer
{
    public ObservableCollection<FighterViewModel> Fighters { get; set; } = [];
    public DataGridCollectionView SortedFighters { get; }

    [ObservableProperty]
    private FighterViewModel? current;

    [ObservableProperty]
    private string _searchText = string.Empty;
    partial void OnSearchTextChanged(string value)
    {
        SortedFighters.Refresh();
    }

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

    private void LoadFighters()
    {
        if (Design.IsDesignMode)
            return;

        try
        {
            Fighters = [.. StorageService.ReadAllFighters().Select(f => new FighterViewModel() { Fighter = f })];
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error reading all fighters: {e}");
        }
    }

    [RelayCommand]
    private void ClearSearch() => SearchText = "";

    [RelayCommand]
    private static void GoHome()
    {
        WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new HomeViewModel()));
    }

    [RelayCommand]
    private async Task NewFighter()
    {
        try
        {
            var result = await DialogBox(new FighterViewModel());
            if (result.IsOk)
            {
                Fighters.Add(result.Item);
                StorageService.WriteFighter(result.Item.Fighter);
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
            var result = await DialogBox(new FighterViewModel() { Fighter = new(item.Fighter) }, DialogButton.Delete);
            if (result.IsOk)
            {
                item.Fighter = result.Item.Fighter;
                StorageService.WriteFighter(result.Item.Fighter);
            }
            else if (result.IsDeleted)
            {
                Fighters.Remove(item);
                StorageService.DeleteFighter(result.Item.Fighter);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error creating and saving a new fighter: {e}");
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
            foreach(var fighter in fighters)
            {
                var existing = Fighters.FirstOrDefault(f => f.Fighter.OnlineId == fighter.OnlineId ||
                    (string.Compare(f.Fighter.FirstName, fighter.FirstName, true) == 0 &&
                     string.Compare(f.Fighter.LastName, fighter.LastName, true) == 0)
                    );
                if (existing is not null)
                    existing.Fighter.UpdateFrom(fighter);
                else
                {
                    StorageService.WriteFighter(fighter);
                    fightersToAdd.Add(fighter);
                }
            }

            // we add new fighters at the end so the above loop doesn't have to search new additions
            foreach(var fighter in fightersToAdd)
                Fighters.Add(new FighterViewModel() { Fighter = fighter });
        }
        else
            MessageBox(message);
    }

    public int Compare(object? x, object? y)
    {
        if (x is FighterViewModel fvm1 && y is FighterViewModel fvm2)
        {
            var lastNameComparison = string.Compare(fvm1.LastName, fvm2.LastName, StringComparison.OrdinalIgnoreCase);
            if (lastNameComparison != 0)
                return lastNameComparison;
            var firstNameComparison = string.Compare(fvm1.FirstName, fvm2.FirstName, StringComparison.OrdinalIgnoreCase);
            if (firstNameComparison != 0)
                return firstNameComparison;
            return (fvm1.OnlineId ?? string.Empty).CompareTo(fvm2.OnlineId ?? string.Empty);
        }
        else
        {
            throw new ArgumentException("Both parameters must be of type FighterViewModel.");
        }
    }

    //[GeneratedRegex(@"\b([a-eu])\b", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    //private static partial Regex RankRegex();

    //[GeneratedRegex(@"\b(rey|ren|tano|dyad)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    //private static partial Regex ClassRegex();
}
