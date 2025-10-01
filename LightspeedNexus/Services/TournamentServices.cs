using LightspeedNexus.Models;
using System.Text;

namespace LightspeedNexus.Services;

public static class TournamentServices
{
    public static string BuildName(Demographic demographic, SkillLevel skillLevel, bool rey, bool ren, bool tano, string? subTitle)
    {
        StringBuilder sb = new();

        if (demographic != Demographic.All)
        {
            sb.Append(demographic.ToString());
            sb.Append("'s ");
        }

        sb.Append(skillLevel.ToString());

        if (rey && ren && tano)
            sb.Append(" Mixed Weapons");
        else if (rey && ren)
            sb.Append(" Rey/Ren");
        else if (rey && tano)
            sb.Append(" Rey/Tano");
        else if (ren && tano)
            sb.Append(" Ren/Tano");
        else if (rey)
            sb.Append(" Rey");
        else if (ren)
            sb.Append(" Ren");
        else if (tano)
            sb.Append(" Tano");

        if (!string.IsNullOrEmpty(subTitle))
            sb.Append($" - {subTitle}");

        return sb.ToString();
    }
}
