using System;

namespace LightspeedNexus.Models;

/// <summary>
/// The settings for a bracket
/// </summary>
public class BracketSettings
{
    public MatchSettings MatchSettings { get; set; } = new();
    public bool HasThirdPlaceMatch { get; set; } = true;
    public bool IsFullAdvancement { get; set; } = true;
}
