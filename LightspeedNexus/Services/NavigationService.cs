using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.ViewModels;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Services;

#region Messages

public class NavigatePageMessage(ViewModelBase viewModel)
{
    public ViewModelBase Page = viewModel;
}

public sealed class StageChangedMessage(StageViewModel? oldStage, StageViewModel? newStage)
{
    public StageViewModel? OldStage { get; } = oldStage;
    public StageViewModel? NewStage { get; } = newStage;
}

public class NavigateHomeMessage() { }

#endregion

public class NavigationService(IServiceProvider serviceProvider, IMessenger messenger)
{
    /// <summary>
    /// Navigates to the given view model
    /// </summary>
    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = serviceProvider.GetRequiredService<TViewModel>();
        messenger.Send(new NavigatePageMessage(viewModel));
    }

    /// <summary>
    /// Navigates to the home page
    /// </summary>
    public void NavigateToTournament(TournamentViewModel tournament) =>
        messenger.Send(new NavigatePageMessage(tournament));

    /// <summary>
    /// Navigates to the home page
    /// </summary>
    public void NavigateToHome() => messenger.Send(new NavigateHomeMessage());

    /// <summary>
    /// This routine will examine the current stage and determine what the next stage is, then navigate to it.
    /// This allows us to keep the view models decoupled from each other, while still allowing for a linear navigation flow.
    /// </summary>
    public void NextStage(StageViewModel currentStage)
    {
        StageViewModel? nextStage = currentStage.GetTransitionToNextStage().GenerateNextStage(currentStage);

        if (nextStage is null)
            messenger.Send(new NavigateHomeMessage());
        else
        {
            currentStage.Next = nextStage;
            nextStage.Previous = currentStage;
            messenger.Send(new StageChangedMessage(currentStage, nextStage));
        }
    }

    /// <summary>
    /// Returns to the previous stage, notifying interested parties of the change.
    /// This allows us to keep the view models decoupled from each other, while still allowing for a linear navigation flow.
    /// </summary>
    public void PreviousStage(StageViewModel currentStage)
    {
        currentStage.Previous?.Next = null;
        messenger.Send(new StageChangedMessage(currentStage, currentStage.Previous));
    }
}