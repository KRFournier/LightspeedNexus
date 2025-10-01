using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// The tournament settings
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    #region Model

    private Settings _model = new();
    public Settings Model
    {
        get => _model;
        set
        {
            if (_model != value)
            {
                _model = value;
                PoolSettings = new MatchSettingsViewModel() { Model = value.PoolSettings };
                BracketSettings = new BracketSettingsViewModel() { Model = value.BracketSettings };
                OnPropertyChanged(nameof(Model));
                OnPropertyChanged(nameof(Date));
                OnPropertyChanged(nameof(Demographic));
                OnPropertyChanged(nameof(SkillLevel));
                OnPropertyChanged(nameof(ReyAllowed));
                OnPropertyChanged(nameof(RenAllowed));
                OnPropertyChanged(nameof(TanoAllowed));
                OnPropertyChanged(nameof(SubTitle));
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public DateTime? Date
    {
        get => Model.Date;
        set
        {
            if (Model.Date != value)
            {
                Model.Date = value;
                OnPropertyChanged(nameof(Date));
            }
        }
    }

    [ObservableProperty]
    private MatchSettingsViewModel _poolSettings = new();

    [ObservableProperty]
    private BracketSettingsViewModel _BracketSettings = new();

    public Demographic Demographic
    {
        get => Model.Demographic;
        set
        {
            if (Model.Demographic != value)
            {
                Model.Demographic = value;
                OnPropertyChanged(nameof(Demographic));
                OnPropertyChanged(nameof(Name));
            }
        }
    }
    public static string[] Demographics => Enum.GetNames(typeof(Demographic));

    public SkillLevel SkillLevel
    {
        get => Model.SkillLevel;
        set
        {
            if (Model.SkillLevel != value)
            {
                Model.SkillLevel = value;
                OnPropertyChanged(nameof(SkillLevel));
                OnPropertyChanged(nameof(Name));
            }
        }
    }
    public static string[] SkillLevels => Enum.GetNames(typeof(SkillLevel));

    public bool ReyAllowed
    {
        get => Model.ReyAllowed;
        set
        {
            if (Model.ReyAllowed != value)
            {
                Model.ReyAllowed = value;
                OnPropertyChanged(nameof(ReyAllowed));
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public bool RenAllowed
    {
        get => Model.RenAllowed;
        set
        {
            if (Model.RenAllowed != value)
            {
                Model.RenAllowed = value;
                OnPropertyChanged(nameof(RenAllowed));
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public bool TanoAllowed
    {
        get => Model.TanoAllowed;
        set
        {
            if (Model.TanoAllowed != value)
            {
                Model.TanoAllowed = value;
                OnPropertyChanged(nameof(TanoAllowed));
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public string SubTitle
    {
        get => Model.SubTitle ?? string.Empty;
        set
        {
            if (Model.SubTitle != value)
            {
                Model.SubTitle = value;
                OnPropertyChanged(nameof(SubTitle));
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    #endregion

    /// <summary>
    /// The name of the tournament, e.g., Open Rey
    /// </summary>
    public string Name => TournamentServices.BuildName(Demographic, SkillLevel, ReyAllowed, RenAllowed, TanoAllowed, SubTitle);
}
