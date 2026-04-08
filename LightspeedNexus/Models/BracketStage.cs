namespace LightspeedNexus.Models;

public sealed class BracketStage : Stage
{
    public MatchGroup Top64 { get; set; } = new();
    public MatchGroup Top32 { get; set; } = new();
    public MatchGroup Top16 { get; set; } = new();
    public MatchGroup Quarterfinals { get; set; } = new();
    public MatchGroup Semifinals { get; set; } = new();
    public MatchGroup Final { get; set; } = new();
    public MatchGroup Third { get; set; } = new();
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Given a number of players, find the bracket we want to work with. This will either
    /// round up to the nearest power of 2 (if 100% advance in enabled) or down to the
    /// nearest power of 2 (if 100% advance is disabled).
    /// </summary>
    public static int FindBracketCount(int numPlayers, bool isFullAdvancement)
    {
        if (isFullAdvancement)
            return numPlayers switch { > 32 => 64, > 16 => 32, > 8 => 16, > 4 => 8, _ => 4 };
        else
            return numPlayers switch { >= 64 => 64, >= 32 => 32, >= 16 => 16, >= 8 => 8, _ => 4 };
    }

    /// <summary>
    /// Predefined loadouts for optimal bracket seeding
    /// </summary>
    public static readonly Dictionary<int, (int, int)[]> Loadouts = new() {
        { 2, [(0, 1)] },
        { 4, [(0, 3), (1, 2)] },
        { 8, [(0, 7), (3, 4), (2, 5), (1, 6)] },
        { 16, [(0, 15), (7, 8), (3, 12), (4, 11), (1, 14), (6, 9), (2, 13), (5, 10)] },
        { 32, [(0, 31), (15, 16), (8, 23), (7, 24), (3, 28), (12, 19), (11, 20), (4, 27), (1, 30), (14, 17), (9, 22), (6, 25), (2, 29), (13, 18), (10, 21), (5, 26)] },
        { 64, [(0, 63), (31, 32), (16, 47), (15, 48), (8, 55), (23, 40), (24, 39), (7, 56), (3, 60), (28, 35), (19, 44), (12, 51), (11, 52), (50, 43), (27, 36), (4, 59), (1, 62), (30, 33), (17, 46), (14, 49), (9, 54), (22, 41), (25, 38), (6, 57), (2, 61), (29, 34), (18, 47), (13, 50), (10, 53), (21, 42), (26, 37), (5, 58)] }
    };
}
