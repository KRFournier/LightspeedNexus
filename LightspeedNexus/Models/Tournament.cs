using System;
using System.Text;

namespace LightspeedNexus.Models;

public sealed class Tournament : CollectionObject
{
    public SetupStage SetupStage { get; set; }
    public bool IsCompleted { get; set; }

    public Tournament()
    {
        SetupStage = new SetupStage();
    }

    public Tournament(Guid id, SetupStage setupStage, bool isCompleted) : base(id)
    {
        SetupStage = setupStage;
        IsCompleted = isCompleted;
    }

    /// <summary>
    /// The name of the tournament, e.g., Open Rey
    /// </summary>
    public static string GetTitle(Demographic demographic, SkillLevel skillLevel,
        GameMode gameMode, bool reyAllowed, bool renAllowed, bool tanoAllowed,
        string? subTitle)
    {
        StringBuilder sb = new();

        if (demographic != Demographic.All)
        {
            sb.Append(demographic.ToString());
            sb.Append("'s ");
        }

        sb.Append(skillLevel.ToString());

        if (gameMode != GameMode.Standard)
        {
            sb.Append(' ');
            sb.Append(gameMode.ToString());
        }

        if (reyAllowed && renAllowed && tanoAllowed)
            sb.Append(" Mixed Weapons");
        else if (reyAllowed && renAllowed)
            sb.Append(" Rey/Ren");
        else if (reyAllowed && tanoAllowed)
            sb.Append(" Rey/Tano");
        else if (renAllowed && tanoAllowed)
            sb.Append(" Ren/Tano");
        else if (reyAllowed)
            sb.Append(" Rey");
        else if (renAllowed)
            sb.Append(" Ren");
        else if (tanoAllowed)
            sb.Append(" Tano");

        if (!string.IsNullOrEmpty(subTitle))
        {
            sb.Append($" - ");
            sb.Append(subTitle);
        }

        return sb.ToString();
    }
}
