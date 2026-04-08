using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using LightspeedNexus.Transitions;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A tournament stage. Base class for all stages
/// </summary>
public abstract partial class StageViewModel(IServiceProvider serviceProvider, IMessenger messenger, NavigationService navigationService) : ViewModelBase(serviceProvider, messenger)
{
    #region Properties

    /// <summary>
    /// The name of this stage
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// The next stage of the tournament. Setting this automatically
    /// goes to that stage.
    /// </summary>
    [ObservableProperty]
    public partial StageViewModel? Next { get; set; }

    /// <summary>
    /// The previous stage of the tournament
    /// </summary>
    [ObservableProperty]
    public partial StageViewModel? Previous { get; set; }

    /// <summary>
    /// When a stage represents the final stage of a tournament, this will be true
    /// </summary>
    public virtual bool IsTournamentCompleted => false;

    /// <summary>
    /// Gets the title of the tournament from the previous stage
    /// </summary>
    public virtual string? Event => Previous?.Event;

    /// <summary>
    /// Gets the title of the tournament from the previous stage
    /// </summary>
    public virtual string Title => Previous?.Title ?? string.Empty;

    #endregion

    /// <summary>
    /// Converts into a model
    /// </summary>
    public abstract Stage ToModel();

    /// <summary>
    /// Called when the tournament is saved
    /// </summary>
    public virtual void OnTournamentSaved() => Next?.OnTournamentSaved();

    /// <summary>
    /// Helper function to get transitions from the service provider
    /// </summary>
    protected T NewTransition<T>() where T : class, IStageTransition => New<T>();

    /// <summary>
    /// Looks for the given previous stage. Returns null if not found
    /// </summary>
    public T? FindPreviousStage<T>() where T : StageViewModel
    {
        var prev = Previous;
        while (prev is not null)
        {
            if (prev is T target)
                return target;
            prev = prev.Previous;
        }
        return null;
    }

    #region Next Navigation

    /// <summary>
    /// Returns to the previous stage
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void GoNext()
    {
        OnGoingNext();
        navigationService.NextStage(this);
    }

    /// <summary>
    /// Determines whether navigation to a previous item is possible.
    /// </summary>
    protected virtual bool CanGoNext() => true;

    /// <summary>
    /// Called before going back to the previous stage
    /// </summary>
    protected virtual void OnGoingNext() { }

    /// <summary>
    /// While stages don't know details about the next stage, they do know what the next stage ought to be
    /// based on their current state. This method returns a transition that can be used to generate the next stage.
    /// </summary>
    public abstract IStageTransition GetTransitionToNextStage();

    #endregion

    #region Back Navigation

    /// <summary>
    /// Returns to the previous stage
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        OnGoingBack();
        navigationService.PreviousStage(this);
    }

    /// <summary>
    /// Determines whether navigation to a previous item is possible.
    /// </summary>
    protected virtual bool CanGoBack() => true;

    /// <summary>
    /// Called before going back to the previous stage
    /// </summary>
    protected virtual void OnGoingBack() { }

    #endregion
}
