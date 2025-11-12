using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Messages;

public class NextStageMessage(StageViewModel current, StageViewModel next)
{
    public StageViewModel CurrentStage { get; } = current;
    public StageViewModel NextStage { get; } = next;
}

public class PreviousStageMessage(StageViewModel current, StageViewModel? previous)
{
    public StageViewModel CurrentStage { get; } = current;
    public StageViewModel? PreviousStage { get; } = previous;
}