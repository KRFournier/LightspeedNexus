using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// The tournament settings
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial DateTime? Date { get; set; } = null;

    [ObservableProperty]
    public partial MatchSettingsViewModel PoolSettings { get; set; } = new();

    [ObservableProperty]
    public partial BracketSettingsViewModel BracketSettings { get; set; } = new();

    [ObservableProperty]
    public partial Demographic Demographic { get; set; } = Demographic.All;
    public static string[] Demographics => Enum.GetNames<Demographic>();

    [ObservableProperty]
    public partial SkillLevel SkillLevel { get; set; } = SkillLevel.Open;
    public static string[] SkillLevels => Enum.GetNames<SkillLevel>();

    [ObservableProperty]
    public partial bool ReyAllowed { get; set; } = true;

    [ObservableProperty]
    public partial bool RenAllowed { get; set; } = false;

    [ObservableProperty]
    public partial bool TanoAllowed { get; set; } = false;

    [ObservableProperty]
    public partial string? SubTitle { get; set; } = null;

    #endregion

    /// <summary>
    /// Creates brand new settings
    /// </summary>
    public SettingsViewModel() { }

    /// <summary>
    /// Loads settings from a model
    /// </summary>
    public SettingsViewModel(Settings model)
    {
        Date = model.Date;
        PoolSettings = model.PoolSettings.ToViewModel();
        BracketSettings = model.BracketSettings.ToViewModel();
        Demographic = model.Demographic;
        SkillLevel = model.SkillLevel;
        ReyAllowed = model.ReyAllowed;
        RenAllowed = model.RenAllowed;
        TanoAllowed = model.TanoAllowed;
        SubTitle = model.SubTitle;
    }

    /// <summary>
    /// Converts into a model
    /// </summary>
    public Settings ToModel() => new(
        Date, PoolSettings.ToModel(), BracketSettings.ToModel(), Demographic,
        SkillLevel, RenAllowed, RenAllowed, TanoAllowed, SubTitle
        );
}
