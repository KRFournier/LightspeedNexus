using System;

namespace LightspeedNexus.Models;

/// <summary>
/// The settings for a set of matches
/// </summary>
public class MatchSettings
{
    public bool IsLocked { get; set; } = false;
    public int WinningScore { get; set; } = 12;
    public TimeSpan TimeLimit { get; set; } = TimeSpan.FromSeconds(90);
}
