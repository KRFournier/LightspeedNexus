using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class EventViewModel : ViewModelBase
{
    #region Model

    private Event _model = new();
    public Event Model
    {
        get => _model;
        set
        {
            if (_model != value)
            {
                OnPropertyChanging(nameof(Model));
                OnPropertyChanging(nameof(Type));
                OnPropertyChanging(nameof(Name));
                OnPropertyChanging(nameof(Start));
                _model = value;
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Start));
            }
        }
    }

    public EventType Type
    {
        get => Model.Type;
        set
        {
            if (Model.Type != value)
            {
                OnPropertyChanging(nameof(Type));
                Model.Type = value;
                OnPropertyChanged(nameof(Type));
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

    public int Day
    {
        get => Model.Day;
        set
        {
            if (Model.Day != value)
            {
                OnPropertyChanging(nameof(Day));
                Model.Day = value;
                OnPropertyChanged(nameof(Day));
            }
        }
    }

    public TimeOnly Start
    {
        get => Model.Start;
        set
        {
            if (Model.Start != value)
            {
                OnPropertyChanging(nameof(Start));
                OnPropertyChanging(nameof(End));
                Model.Start = value;
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(End));
            }
        }
    }

    public TimeSpan Duration => Model.Duration;

    #endregion

    public TimeOnly End => TimeOnly.FromTimeSpan(Start.ToTimeSpan() + Duration);
}
