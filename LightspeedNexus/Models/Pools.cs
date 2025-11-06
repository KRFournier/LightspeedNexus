namespace LightspeedNexus.Models;

/// <summary>
/// A player's performance in the qualifiers.
/// </summary>
public class PoolStat
{
    public int? Seed { get; set; }
    public int Player { get; set; } = -1;
    public int Wins { get; set; } = 0;
    public double? Score { get; set; }

    public PoolStat() { }

    public PoolStat(int? seed, int player, int wins, double? score)
    {
        Seed = seed;
        Player = player;
        Wins = wins;
        Score = score;
    }
}

/// <summary>
/// A round robin pool of matches.
/// </summary>
public class Pool
{
    public int Squadron { get; set; } = -1;
    public Match[] Matches { get; set; } = [];

    public Pool() { }

    public Pool(int squadron, Match[] matches)
    {
        Squadron = squadron;
        Matches = matches;
    }
}


/// <summary>
/// A set of round robin pools. The results of these matches are used to seed the bracket
/// for the next stage of the tournament.
/// </summary>
public class Qualifier
{
    public Pool[] Pools { get; set; } = [];
    public PoolStat[] Rankings { get; set; } = [];

    public Qualifier() { }

    public Qualifier(Pool[] pools, PoolStat[] rankings)
    {
        Pools = pools;
        Rankings = rankings;
    }
}