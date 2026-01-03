using System;

namespace LightspeedNexus.Models;

public class Seed
{
    public int Place { get; set; }
    public Guid Participant { get; set; }
    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public double Score { get; set; } = 0.0;
}

public sealed class SeedingStage : Stage
{
    public Seed[] Seeds { get; set; } = [];
}
