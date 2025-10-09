using LightspeedNexus.ViewModels;
using System;

namespace LightspeedNexus.Models;

public sealed record Tournament(Guid Id,
    bool IsStarted,
    Settings Settings,
    Roster Roster) : CollectionObject(Id)
{
    public TournamentViewModel ToViewModel() => new(this);
}
