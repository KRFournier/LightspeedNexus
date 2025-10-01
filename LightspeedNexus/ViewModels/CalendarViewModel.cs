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

    private Competition _competition = new();
    public Competition Competition
    {
        get => _competition;
        set
        {
            if (_competition != value)
            {
                _competition = value;
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(Days));
                OnPropertyChanged(nameof(DisplayDates));
                Events = [.._competition.Events.Select(e => new EventViewModel() { Model = e })];
                OnPropertyChanged(nameof(MinTime));
                OnPropertyChanged(nameof(MaxTime));
            }
        }
    }

    public string Name
    {
        get => Competition.Name;
        set
        {
            if (Competition.Name != value)
            {
                Competition.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public DateTime Start
    {
        get => Competition.Start.ToDateTime(TimeOnly.MinValue);
        set
        {
            var dateOnlyValue = DateOnly.FromDateTime(value);
            if (Competition.Start != dateOnlyValue)
            {
                Competition.Start = dateOnlyValue;
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(DisplayDates));
                OnPropertyChanged(nameof(MinTime));
                OnPropertyChanged(nameof(MaxTime));
            }
        }
    }

    public int Days
    {
        get => Competition.Days;
        set
        {
            if (Competition.Days != value)
            {
                Competition.Days = value;
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
            var result = await DialogBox(new CompetitionViewModel() { Model = new(Competition) }, "Edit Competition");
            if (result.IsOk)
            {
                Competition = result.Item.Model;
                //StorageService.WriteFighter(result.Item.Model);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error editing the competition: {e}");
        }
    }
}
