using System;

namespace LightspeedNexus.Models;

public sealed class Statistics
{
    public int Place { get; set; } = 0;
    public Guid Participant { get; set; } = Guid.Empty;
    public int Wins { get; set; } = 0;
    public int Points { get; set; } = 0;
    public double PossiblePoints { get; set; } = 0.0;
    public double Value { get; set; } = 0.0;
    public Rank? OldRank { get; set; }
    public Rank? NewRank { get; set; }
}

public sealed class ResultsStage : Stage
{
    public Statistics[] Placements { get; set; } = [];
}
