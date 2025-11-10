using System;

namespace LightspeedNexus.Models;

public sealed class Tournament : CollectionObject
{
    public SetupStage SetupStage { get; set; }
    public bool IsCompleted { get; set; }

    public Tournament()
    {
        SetupStage = new SetupStage();
    }

    public Tournament(Guid id, SetupStage setupStage, bool isCompleted) : base(id)
    {
        SetupStage = setupStage;
        IsCompleted = isCompleted;
    }
}
