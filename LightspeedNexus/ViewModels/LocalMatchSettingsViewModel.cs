using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// Match settings for a specific group of matches that can be different from the global settings
/// </summary>
public partial class LocalMatchSettingsViewModel : ViewModelBase, IDisposable
{
    private MatchSettingsViewModel? _localSettings = null;

    #region Properties

    /// <summary>
    /// The global settings from which these settings derive
    /// </summary>
    [ObservableProperty]
    public partial MatchSettingsViewModel GlobalSettings { get; protected set; }

    /// <summary>
    /// Determines whether these settings are overriding the global settings
    /// </summary>
    public bool IsOverridden => _localSettings is not null;

    /// <summary>
    /// The points needed to win a match in this pool
    /// </summary>
    public int WinningScore
    {
        get => _localSettings?.WinningScore ?? GlobalSettings.WinningScore;
        set
        {
            // if there are no local settings and we're diverging from global settings
            // create local settings
            if (_localSettings is null && value != GlobalSettings.WinningScore)
            {
                OnPropertyChanging(nameof(WinningScore));
                _localSettings = new MatchSettingsViewModel()
                {
                    WinningScore = value,
                    TimeLimit = GlobalSettings.TimeLimit
                };
                OnPropertyChanged(nameof(WinningScore));
                OnPropertyChanged(nameof(IsOverridden));
            }

            // if there are local settings, see if we're returning to global settings
            // in which case we can remove local settings
            // otherwise just update local settings
            else if (_localSettings is not null && _localSettings.WinningScore != value)
            {
                OnPropertyChanging(nameof(WinningScore));
                _localSettings.WinningScore = value;
                CheckIfLocalAndGlobalMatch();
                OnPropertyChanged(nameof(WinningScore));
            }
        }
    }

    /// <summary>
    /// The time limit for matches in this pool
    /// </summary>
    public TimeSpan TimeLimit
    {
        get => _localSettings?.TimeLimit ?? GlobalSettings.TimeLimit;
        set
        {
            // if there are no local settings and we're diverging from global settings
            // create local settings
            if (_localSettings is null && value != GlobalSettings.TimeLimit)
            {
                OnPropertyChanging(nameof(TimeLimit));
                _localSettings = new MatchSettingsViewModel()
                {
                    WinningScore = GlobalSettings.WinningScore,
                    TimeLimit = value
                };
                OnPropertyChanged(nameof(TimeLimit));
                OnPropertyChanged(nameof(IsOverridden));
            }

            // if there are local settings, see if we're returning to global settings
            // in which case we can remove local settings
            // otherwise just update local settings
            else if (_localSettings is not null && _localSettings.TimeLimit != value)
            {
                OnPropertyChanging(nameof(TimeLimit));
                _localSettings.TimeLimit = value;
                CheckIfLocalAndGlobalMatch();
                OnPropertyChanged(nameof(TimeLimit));
            }
        }
    }

    #endregion

    /// <summary>
    /// Creates new local settings that derive from the given global settings
    /// </summary>
    public LocalMatchSettingsViewModel(MatchSettingsViewModel globalSettings)
    {
        GlobalSettings = globalSettings;
        GlobalSettings.PropertyChanged += ParentPropertyChanged;
    }

    /// <summary>
    /// Necessary to ensure we unsubscribe from the global setting's PropertyChanged event
    /// </summary>
    public void Dispose()
    {
        GlobalSettings.PropertyChanged -= ParentPropertyChanged;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// When the global settings change, we need to see if they match our local settings
    /// </summary>
    public void ParentPropertyChanged(object? sender, PropertyChangedEventArgs e) => CheckIfLocalAndGlobalMatch();

    /// <summary>
    /// Checks if the local settings match the global settings, and if so, removes the local settings
    /// </summary>
    private void CheckIfLocalAndGlobalMatch()
    {
        if (_localSettings is not null && _localSettings == GlobalSettings)
        {
            OnPropertyChanging(nameof(WinningScore));
            OnPropertyChanging(nameof(TimeLimit));
            _localSettings = null;
            OnPropertyChanged(nameof(IsOverridden));
            OnPropertyChanged(nameof(WinningScore));
            OnPropertyChanged(nameof(TimeLimit));
        }
    }
}
