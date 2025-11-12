namespace LightspeedNexus.Models;

/// <summary>
/// A round robin pool of matches.
/// </summary>
public class Pool
{
    public int Squadron { get; set; } = -1;
    //public Match[] Matches { get; set; } = [];

    public Pool() { }

    public Pool(int squadron) //, Match[] matches)
    {
        Squadron = squadron;
        //Matches = matches;
    }
}

/// <summary>
/// A set of round robin pools. The results of these matches are used to seed the bracket
/// for the next stage of the tournament.
/// </summary>
public class PoolsStage : Stage
{
    public Pool[] Pools { get; set; } = [];

    public PoolsStage() { }

    public PoolsStage(Pool[] pools, Stage? next) : base(next)
    {
        Pools = pools;
    }
}