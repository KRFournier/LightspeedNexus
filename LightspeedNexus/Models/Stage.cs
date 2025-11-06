using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Models;

/// <summary>
/// A stage in a tournament.
/// </summary>
public abstract class Stage
{
    public string Type => GetType().Name;

    public string Name { get; set; }

    public Stage()
    {
        Name = GetType().Name.Replace("Stage", "");
    }

    public Stage(string name)
    {
        Name = name;
    }

    abstract public StageViewModel ToViewModel();
}
