namespace LightspeedNexus.Models;

/// <summary>
/// A stage in a tournament.
/// </summary>
public abstract class Stage(Stage? next = null)
{
    /// <summary>
    /// Gets or sets the next stage in the sequence, if one exists.
    /// </summary>
    public Stage? Next { get; set; } = next;
}
