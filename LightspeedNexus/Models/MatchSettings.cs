using System;

namespace LightspeedNexus.Models;

/// <summary>
/// The settings for a set of matches
/// </summary>
public class MatchSettings
{
    public int WinningScore { get; set; } = 12;
    public TimeSpan TimeLimit { get; set; } = TimeSpan.FromSeconds(90);

    public MatchSettings() { }

    public MatchSettings(int winningScore, TimeSpan timeLimit)
    {
        WinningScore = winningScore;
        TimeLimit = timeLimit;
    }
}