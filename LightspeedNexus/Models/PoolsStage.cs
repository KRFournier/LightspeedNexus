namespace LightspeedNexus.Models;

/// <summary>
/// A round robin pool of matches.
/// </summary>
public class Pool
{
    public Guid Squadron { get; set; } = Guid.NewGuid();
    public MatchGroup MatchGroup { get; set; } = new();
}

/// <summary>
/// A set of round robin pools. The results of these matches are used to seed the bracket
/// for the next stage of the tournament.
/// </summary>
public class PoolsStage : Stage
{
    public Pool[] Pools { get; set; } = [];
}