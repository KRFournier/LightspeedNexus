using Lightspeed.ViewModels;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Transitions;

/// <summary>
/// Transitions from the setup stage to the squadrons stage, which means that each registree is a participant
/// </summary>
public class SetupToSquadronsTransition(IServiceProvider serviceProvider) : IStageTransition
{
    public StageViewModel GenerateNextStage(StageViewModel currentStage)
    {
        if (currentStage is not SetupStageViewModel setupStage)
        {
            throw new InvalidOperationException("Current stage must be a SetupStageViewModel");
        }

        // convert the registrees to player view models
        IEnumerable<ParticipantViewModel> participants = setupStage.Registrees.Select(r =>
        {
            var vm = serviceProvider.GetRequiredService<PlayerViewModel>();
            vm.FirstName = r.FirstName;
            vm.LastName = r.LastName;
            vm.OnlineId = r.OnlineId;
            vm.Club = r.Club;
            vm.Rank = r.Rank;
            vm.WeaponOfChoice = r.WeaponOfChoice;
            return vm;
        });

        // go to squadrons stage
        var stage = serviceProvider.GetRequiredService<SquadronsStageViewModel>();
        foreach (var participant in participants)
            stage.Participants.Add(participant);
        stage.Refresh();

        return stage;
    }
}
