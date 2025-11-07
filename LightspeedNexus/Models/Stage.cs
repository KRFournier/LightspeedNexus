using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Models;

/// <summary>
/// A stage in a tournament.
/// </summary>
public abstract class Stage
{
    public string Type => GetType().Name;

    public Stage() { }

    abstract public StageViewModel ToViewModel();
}
