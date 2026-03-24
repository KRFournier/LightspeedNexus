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
}
