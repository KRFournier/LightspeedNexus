using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A single score, used to edit matches
/// </summary>
public partial class ScoreEditViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int Points { get; set; } = 0;

    #endregion

    #region Commands

    [RelayCommand]
    private void Increment() => Points++;

    [RelayCommand]
    private void Decrement()
    {
        if (Points > 0)
            Points--;
    }

    #endregion
}

/// <summary>
/// A match's scores, used to edit matches
/// </summary>
public partial class MatchEditViewModel : ViewModelBase
{
    #region Properties

    public ObservableCollection<ScoreEditViewModel> First { get; set; } = [];

    public ObservableCollection<ScoreEditViewModel> Second { get; set; } = [];

    #endregion
}
