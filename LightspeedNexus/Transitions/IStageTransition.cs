using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Transitions;

/// <summary>
/// Stage transitions perform the actual work of transitioning from one stage to another.
/// They are responsible for any data manipulation, API calls, or other side effects that need to happen
/// when moving from one stage to the next new stage.
/// </summary>
public interface IStageTransition
{
    public StageViewModel? GenerateNextStage(StageViewModel currentStage);
}
