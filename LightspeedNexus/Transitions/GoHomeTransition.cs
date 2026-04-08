using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Transitions;

/// <summary>
/// Used by view models that want the Next button to go back to the home screen, which is represented by returning null from GenerateNextStage
/// </summary>
public class GoHomeTransition : IStageTransition
{
    public StageViewModel? GenerateNextStage(StageViewModel currentStage) => null;
}
