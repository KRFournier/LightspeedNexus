namespace LightspeedNexus.Models;

/// <summary>
/// A pool of players in a tournament.
/// </summary>
public sealed class Squadron
{
    public int[] Players { get; set; } = [];
    public int Weight { get; set; } = 0;
    public MatchSettings MatchSettings { get; set; } = new();

    public Squadron() { }
    public Squadron(int[] players, int weight, MatchSettings settings)
    {
        Players = players;
        Weight = weight;
        MatchSettings = settings;
    }
}

/// <summary>
/// The settings for a tournament
/// </summary>
public sealed class SquadronsStage : Stage
{
    public Participant[] Participants { get; set; } = [];
    public bool IsAutoAssigned { get; set; } = true;
    public Squadron[] Squadrons { get; set; } = [];

    public SquadronsStage() { }
    public SquadronsStage(bool isAutoAssigned, Participant[] participants, Squadron[] squadrons)
    {
        IsAutoAssigned = isAutoAssigned;
        Participants = participants;
        Squadrons = squadrons;
    }
}
