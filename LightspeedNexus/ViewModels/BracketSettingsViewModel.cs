using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// All the settings for the bracket
/// </summary>
public partial class BracketSettingsViewModel : MatchSettingsViewModel
{
    #region Properties

    /// <summary>
    /// Determines if the brackets will have a third place match
    /// </summary>
    [ObservableProperty]
    public partial bool HasThirdPlaceMatch { get; set; }

    /// <summary>
    /// Determines if all players advance to the brackets, or just the top X players
    /// </summary>
    [ObservableProperty]
    public partial bool IsFullAdvancement { get; set; }

    #endregion

    /// <summary>
    /// Creates brand new bracket settings
    /// </summary>
    public BracketSettingsViewModel() { }

    /// <summary>
    /// Loads bracket settings from a model
    /// </summary>
    public BracketSettingsViewModel(BracketSettings model) : base(model)
    {
        HasThirdPlaceMatch = model.HasThirdPlaceMatch;
        IsFullAdvancement = model.IsFullAdvancement;
    }

    /// <summary>
    /// The model
    /// </summary>
    public new BracketSettings ToModel() => new()
    {
        WinningScore = WinningScore,
        TimeLimit = TimeLimit,
        HasThirdPlaceMatch = HasThirdPlaceMatch,
        IsFullAdvancement = IsFullAdvancement
    };

    /// <summary>
    /// The match settingn model
    /// </summary>
    public MatchSettings ToMatchSettings() => base.ToModel();
}
