namespace LightspeedNexus.Models;

/// <summary>
/// A pool of players in a tournament.
/// </summary>
public sealed class Squadron
{
    public int[] Players { get; set; } = [];
    public int Weight { get; set; } = 0;

    public Squadron() { }
    public Squadron(int[] players, int weight)
    {
        Players = players;
        Weight = weight;
    }
}

/// <summary>
/// The settings for a tournament
/// </summary>
public sealed class SquadronsStage : Stage
{
    public Participant[] Participants { get; set; } = [];
    public bool IsAutoAssigned { get; set; } = true;

    public SquadronsStage() { }
    public SquadronsStage(Participant[] participants)
    {
        Participants = participants;
    }
}
