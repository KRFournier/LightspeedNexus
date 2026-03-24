namespace LightspeedNexus.Models;

/// <summary>
/// The settings for a tournament
/// </summary>
public sealed class SquadronsStage : Stage
{
    public IParticipant[] Participants { get; set; } = [];
    public bool IsAutoAssigned { get; set; } = true;
    public Squadron[] Squadrons { get; set; } = [];
}
