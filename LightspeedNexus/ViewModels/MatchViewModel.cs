using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A match
/// </summary>
public partial class MatchViewModel : ViewModelBase
{
    #region Properties

    /// <summary>
    /// The match's unique identifier
    /// </summary>
    public Guid Guid { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// The match's settings, inherited from the parent pool of matches
    /// </summary>
    [ObservableProperty]
    public partial MatchSettingsViewModel Settings { get; set; }

    #endregion

    public MatchViewModel(MatchSettingsViewModel settings)
    {
        Settings = settings;
    }

    public MatchViewModel(Match match, MatchSettingsViewModel settings) : this(settings)
    {

    }
}
