using System;

namespace LightspeedNexus.Models;

/// <summary>
/// A pool of players in a tournament.
/// </summary>
public sealed class Squadron
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    public Guid[] Players { get; set; } = [];
    public int Weight { get; set; } = 0;
    public MatchSettings MatchSettings { get; set; } = new();
}

/// <summary>
/// The settings for a tournament
/// </summary>
public sealed class SquadronsStage : Stage
{
    public Participant[] Participants { get; set; } = [];
    public bool IsAutoAssigned { get; set; } = true;
    public Squadron[] Squadrons { get; set; } = [];
}
