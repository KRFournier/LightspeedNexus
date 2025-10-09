using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class CompetitionViewModel : ViewModelBase
{
    #region Model

    private Competition _model = new();
    public Competition Model
    {
        get => _model;
        set
        {
            if (_model != value)
            {
                OnPropertyChanging(nameof(Model));
                OnPropertyChanging(nameof(Id));
                OnPropertyChanging(nameof(Name));
                OnPropertyChanging(nameof(Start));
                OnPropertyChanging(nameof(Days));
                OnPropertyChanging(nameof(DateRange));
                _model = value;
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(Id));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(Days));
                OnPropertyChanged(nameof(DateRange));
                Roster = [..value.Roster];
            }
        }
    }

    public Guid Id => Model.Id;

    public string Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name != value)
            {
                OnPropertyChanging(nameof(Name));
                Model.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public DateTime Start
    {
        get => Model.Start.ToDateTime(TimeOnly.MinValue);
        set
        {
            var dateOnlyValue = DateOnly.FromDateTime(value);
            if (Model.Start != dateOnlyValue)
            {
                OnPropertyChanging(nameof(Start));
                Model.Start = dateOnlyValue;
                OnPropertyChanged(nameof(Start));
            }
        }
    }

    public int Days
    {
        get => Model.Days;
        set
        {
            if (Model.Days != value)
            {
                OnPropertyChanging(nameof(Days));
                Model.Days = value;
                OnPropertyChanged(nameof(Days));
            }
        }
    }

    #endregion

    public CompetitionViewModel()
    {
        foreach (var venue in StorageService.ReadAll<Venue>())
        {
            Venues.Add(venue.Name);
            _venueLookup[venue.Name] = venue;
        }
        Fighters = [.. StorageService.ReadAll<Fighter>()];
    }

    public CalendarDateRange DateRange
    {
        get => new(Start, Start.AddDays(Days - 1));
        set
        {
            if (value.Start != Start || value.End != Start.AddDays(Days - 1))
            {
                Start = value.Start;
                Days = (int)(value.End - value.Start).TotalDays + 1;
                // NOTE: not notifying property change for DateRange here, as it is derived from Start and Days
                //       so when you bind to this, don't bind to Start and Days on the same form
            }
        }
    }

    [RelayCommand]
    private void IncrementDays()
    {
        Days++;
    }

    [RelayCommand]
    private void DecrementDays()
    {
        if (Days > 1)
            Days--;
    }

    #region Venues

    private readonly Dictionary<string, Venue> _venueLookup = [];

    [ObservableProperty]
    public partial string? SelectedVenue { get; set; }
    partial void OnSelectedVenueChanged(string? value)
    {
        if (_venueLookup.TryGetValue(value ?? string.Empty, out var venue))
            Model.Venue = venue;
        else
            Model.Venue = null;
    }

    public ObservableCollection<string> Venues { get; set; } = [];

    [RelayCommand]
    private async Task NewVenue()
    {
        try
        {
            var result = await DialogBox(new VenueViewModel() { Rings = ["Ring 1"] }, "New Venue");
            if (result.IsOk)
            {
                SelectedVenue = result.Item.Name;
                Venues.Add(SelectedVenue);
                StorageService.Write(result.Item.Model);
                _venueLookup[SelectedVenue] = result.Item.Model;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error creating and saving a new venue: {e}");
        }
    }


    [RelayCommand]
    private async Task EditVenue()
    {
        if (SelectedVenue is null)
        {
            Debug.WriteLine("No venue selected for editing.");
            return;
        }

        try
        {
            if (_venueLookup.TryGetValue(SelectedVenue, out var venue))
            {
                var result = await DialogBox(new VenueViewModel() { Model = new(venue) }, "Edit Venue");
                if (result.IsOk)
                {
                    SelectedVenue = result.Item.Name;
                    StorageService.Write(result.Item.Model);
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error editing the venue: {e}");
        }
    }

    #endregion

    #region Roster

    public ObservableCollection<Fighter> Roster { get; set; } = [];

    [ObservableProperty]
    public partial string? FighterSearchText { get; set; }

    [ObservableProperty]
    public partial Fighter? SelectedFighter { get; set; }
    public ObservableCollection<Fighter> Fighters { get; set; } = [];

    [RelayCommand]
    private static void GoHome() => WeakReferenceMessenger.Default.Send<NavigateHomeMessage>();

    [RelayCommand]
    private async Task AddFighter()
    {
        if (SelectedFighter is not null)
        {
            if (Roster.FirstOrDefault(f => f.Id == SelectedFighter.Id) is null)
            {
                Roster.Add(SelectedFighter);
                Model.Roster.Add(SelectedFighter);
                SelectedFighter = new();
                FighterSearchText = null;
                WeakReferenceMessenger.Default.Send<ClearAutoCompleteMessage>();
            }
        }
        else
        {
            await NewFighter();
        }
    }

    [RelayCommand]
    private async Task NewFighter()
    {
        try
        {
            var result = await DialogBox(new FighterViewModel(), "New Fighter");
            if (result.IsOk)
            {
                Roster.Add(result.Item.Model);
                Model.Roster.Add(result.Item.Model);
                StorageService.Write(result.Item.Model);
                SelectedFighter = new();
                FighterSearchText = null;
                WeakReferenceMessenger.Default.Send<ClearAutoCompleteMessage>();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error creating and saving a new fighter: {e}");
        }
    }

    #endregion
}
