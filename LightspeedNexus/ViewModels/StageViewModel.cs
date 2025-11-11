using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using System.Security;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A tournament stage. Base class for all stages
/// </summary>
public abstract partial class StageViewModel(string name) : ViewModelBase
{
    #region Properties

    /// <summary>
    /// The name of this stage
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = name;

    /// <summary>
    /// The next stage of the tournament. Setting this automatically
    /// goes to that stage.
    /// </summary>
    [ObservableProperty]
    public partial StageViewModel? Next { get; set; }
    partial void OnNextChanged(StageViewModel? value)
    {
        if (value is not null)
        {
            value.Previous = this;
            WeakReferenceMessenger.Default.Send(new NextStageMessage(this, value));
        }
    }

    /// <summary>
    /// The previous stage of the tournament
    /// </summary>
    [ObservableProperty]
    public partial StageViewModel? Previous { get; set; }

    /// <summary>
    /// When a stage represents the final stage of a tournament, this will be true
    /// </summary>
    public virtual bool IsTournamentCompleted => false;

    #endregion

    /// <summary>
    /// Converts into a model
    /// </summary>
    public abstract Stage ToModel();

    /// <summary>
    /// Returns to the previous stage
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack() => WeakReferenceMessenger.Default.Send(new PreviousStageMessage(this, Previous));

    /// <summary>
    /// Determines whether navigation to a previous item is possible.
    /// </summary>
    protected virtual bool CanGoBack() => true;
}
