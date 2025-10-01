using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// All the settings for the bracket
/// </summary>
public partial class BracketSettingsViewModel : ViewModelBase
{
    #region Model

    /// <summary>
    /// The model
    /// </summary>
    private BracketSettings _model = new();
    public BracketSettings Model
    {
        get => _model;
        set
        {
            if (_model != value)
            {
                _model = value;
                MatchSettings = new MatchSettingsViewModel() { Model = value.MatchSettings };
                OnPropertyChanged(nameof(HasThirdPlaceMatch));
                OnPropertyChanged(nameof(IsFullAdvancement));
            }
        }
    }

    /// <summary>
    /// The global settings for bracket matches
    /// </summary>
    [ObservableProperty]
    private MatchSettingsViewModel _matchSettings = new();

    /// <summary>
    /// Determines if the brackets will have a third place match
    /// </summary>
    public bool HasThirdPlaceMatch
    {
        get => Model.HasThirdPlaceMatch;
        set
        {
            if (value != Model.HasThirdPlaceMatch)
            {
                Model.HasThirdPlaceMatch = value;
                OnPropertyChanged(nameof(HasThirdPlaceMatch));
            }
        }
    }

    /// <summary>
    /// Determines if all players advance to the brackets, or just the top X players
    /// </summary>
    public bool IsFullAdvancement
    {
        get => Model.IsFullAdvancement;
        set
        {
            if (value != Model.IsFullAdvancement)
            {
                Model.IsFullAdvancement = value;
                OnPropertyChanged(nameof(IsFullAdvancement));
            }
        }
    }

    #endregion
}
