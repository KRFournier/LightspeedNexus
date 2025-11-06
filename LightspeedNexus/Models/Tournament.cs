using LightspeedNexus.ViewModels;
using System;

namespace LightspeedNexus.Models;

public sealed class Tournament : CollectionObject
{
    public Stage[] Stages { get; set; }

    public Tournament()
    {
        Stages = [new SetupStage()];
    }

    public Tournament(Guid id, Stage[] stages) : base(id)
    {
        Stages = stages;
    }

    public TournamentViewModel ToViewModel() => new(this);
}
