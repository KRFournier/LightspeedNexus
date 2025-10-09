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
using System.Text;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// ViewModel for the calendar, which displays competitions and competition details
/// </summary>
public partial class CalendarViewModel() : ViewModelBase
{
    #region Competition

    private Competition _model = new();
    public Competition Model
    {
        get => _model;
        set
        {
            if (_model != value)
            {
                OnPropertyChanging(nameof(Model));
                OnPropertyChanging(nameof(Name));
                OnPropertyChanging(nameof(Start));
                OnPropertyChanging(nameof(Days));
                OnPropertyChanging(nameof(DisplayDates));
                OnPropertyChanging(nameof(MinTime));
                OnPropertyChanging(nameof(MaxTime));
                _model = value;
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(Days));
                OnPropertyChanged(nameof(DisplayDates));
                OnPropertyChanged(nameof(MinTime));
                OnPropertyChanged(nameof(MaxTime));
                Events = [.. _model.Events.Select(e => new EventViewModel() { Model = e })];
            }
        }
    }

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
                OnPropertyChanging(nameof(DisplayDates));
                OnPropertyChanging(nameof(MinTime));
                OnPropertyChanging(nameof(MaxTime));
                Model.Start = dateOnlyValue;
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(DisplayDates));
                OnPropertyChanged(nameof(MinTime));
                OnPropertyChanged(nameof(MaxTime));
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
                OnPropertyChanging(nameof(DisplayDates));
                OnPropertyChanging(nameof(MinTime));
                OnPropertyChanging(nameof(MaxTime));
                Model.Days = value;
                OnPropertyChanged(nameof(Days));
                OnPropertyChanged(nameof(DisplayDates));
                OnPropertyChanged(nameof(MinTime));
                OnPropertyChanged(nameof(MaxTime));
            }
        }
    }

    public ObservableCollection<EventViewModel> Events { get; set; } = [];

    public string DisplayDates => Days > 1 ?
        $"{Start:MMMM d, yyyy} - {Start.AddDays(Days):MMMM d, yyyy}" :
        $"{Start:MMMM d, yyyy}";

    #endregion

    #region Calendar Stuff

    public TimeOnly MinTime
    {
        get
        {
            var def = TimeOnly.FromDateTime(DateTime.Today.AddHours(8));

            if (Events.Count == 0)
                return def;

            var min = Events.Min(e => e.Start);
            return min < def ? min : def;
        }
    }

    public TimeOnly MaxTime
    {
        get
        {
            var def = TimeOnly.FromDateTime(DateTime.Today.AddHours(20));

            if (Events.Count == 0)
                return def;

            var max = Events.Max(e => TimeOnly.FromTimeSpan(e.Start.ToTimeSpan() + e.Duration));
            return max > def ? max : def;
        }
    }

    public void AddEvent(string name, int day, TimeOnly time, TimeSpan duration)
    {
        OnPropertyChanging(nameof(MinTime));
        OnPropertyChanging(nameof(MaxTime));
        Events.Add(new()
        {
            Name = name,
            Day = day,
            Start = time
        });
        OnPropertyChanged(nameof(MinTime));
        OnPropertyChanged(nameof(MaxTime));
    }

    #endregion

    [RelayCommand]
    private static void GoHome()
    {
        WeakReferenceMessenger.Default.Send<NavigateHomeMessage>();
    }

    [RelayCommand]
    private async Task EditCompetition()
    {
        try
        {
            var result = await DialogBox(new CompetitionViewModel() { Model = new(Model) }, "Edit Competition");
            if (result.IsOk)
            {
                Model = result.Item.Model;
                //StorageService.WriteFighter(result.Item.Model);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error editing the competition: {e}");
        }
    }
}
