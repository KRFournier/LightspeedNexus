using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Messages;

public class StageChangedMessage(StageViewModel stage)
{
    public StageViewModel Stage { get; } = stage;
}
