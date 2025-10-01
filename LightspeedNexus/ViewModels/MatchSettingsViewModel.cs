using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LightspeedNexus.Models;
using System;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A group of matches sharing similar settings
/// </summary>
public partial class MatchSettingsViewModel : ViewModelBase
{
    #region Model

    /// <summary>
    /// The model
    /// </summary>
    private MatchSettings _model = new();
    public MatchSettings Model
    {
        get => _model;
        set
        {
            if (_model != value)
            {
                _model = value;
                OnPropertyChanged(nameof(IsLocked));
                OnPropertyChanged(nameof(WinningScore));
                OnPropertyChanged(nameof(TimeLimit));
            }
        }
    }

    /// <summary>
    /// The points needed to win a match in this pool
    /// </summary>
    public bool IsLocked
    {
        get => Model.IsLocked;
        set
        {
            if (value != Model.IsLocked)
            {
                Model.IsLocked = value;
                OnPropertyChanged(nameof(IsLocked));
            }
        }
    }

    /// <summary>
    /// The points needed to win a match in this pool
    /// </summary>
    public int WinningScore
    {
        get => Model.WinningScore;
        set
        {
            if (value < 0)
                value = 0;
            if (value != Model.WinningScore)
            {
                Model.WinningScore = value;
                OnPropertyChanged(nameof(WinningScore));
            }
        }
    }

    /// <summary>
    /// The time limit for matches in this pool
    /// </summary>
    public TimeSpan TimeLimit
    {
        get => Model.TimeLimit;
        set
        {
            if (value != Model.TimeLimit)
            {
                Model.TimeLimit = value;
                OnPropertyChanged(nameof(TimeLimit));
            }
        }
    }

    #endregion

    /// <summary>
    /// Initializes the settings
    /// </summary>
    public MatchSettingsViewModel()
    {
    }

    /// <summary>
    /// Initializes from parent settings, and sets these settings to 
    /// listen to changes to the parent settings
    /// </summary>
    public MatchSettingsViewModel(MatchSettingsViewModel parent)
    {
        Model = new MatchSettings()
        {
            WinningScore = parent.Model.WinningScore,
            TimeLimit = parent.Model.TimeLimit
        };
    }

    /// <summary>
    /// Increases the winning score by 1 point
    /// </summary>
    [RelayCommand]
    private void IncreaseScore()
    {
        WinningScore++;
    }

    /// <summary>
    /// Decreases the winning score by 1 point
    /// </summary>
    [RelayCommand]
    private void DecreaseScore()
    {
        if (WinningScore > 0)
            WinningScore--;
    }

    /// <summary>
    /// Increases the time limit by 15 seconds
    /// </summary>
    [RelayCommand]
    private void IncreaseTime()
    {
        TimeLimit += TimeSpan.FromSeconds(15);
    }

    /// <summary>
    /// Decreases the time limit by 15 seconds
    /// </summary>
    [RelayCommand]
    private void DecreaseTime()
    {
        if (TimeLimit.TotalSeconds > 15)
            TimeLimit -= TimeSpan.FromSeconds(15);
    }

    /// <summary>
    /// Increases the winning score by 4 points and the time by 30 seconds
    /// </summary>
    [RelayCommand]
    private void Increase()
    {
        int chunk = WinningScore / 4 + 1;
        WinningScore = chunk * 4;
        TimeLimit = TimeSpan.FromSeconds(chunk * 30);
    }

    /// <summary>
    /// Decreases the winning score by 4 points and the time by 30 seconds
    /// </summary>
    [RelayCommand]
    private void Decrease()
    {
        int chunk = WinningScore / 4;
        if (chunk-- > 0)
        {
            WinningScore = chunk * 4;
            TimeLimit = TimeSpan.FromSeconds(chunk * 30);
        }
    }


    #region Saber Sports

    /// <summary>
    /// Creates the json for submitting the tournament to saber-sports
    /// </summary>
    public JsonNode ToSaberSportsSubmission(string name, bool disablePromotion) => new JsonObject
    {
        ["name"] = name,
        ["duration"] = TimeLimit.ToString(),
        ["score"] = WinningScore,
        ["disable_rank_promotion"] = disablePromotion,
    };

    #endregion
}
