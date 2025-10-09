using LightspeedNexus.Controls;
using LightspeedNexus.ViewModels;
using System;

namespace LightspeedNexus.Models;

/// <summary>
/// The tournament's division based on demographics
/// </summary>
public enum Demographic
{
    All,
    Women,
    Cadet
}

/// <summary>
/// The tournament's division based on skill level
/// </summary>
public enum SkillLevel
{
    Open,
    Advanced,
    Novice
}

/// <summary>
/// The settings for a set of matches
/// </summary>
public record MatchSettings(int WinningScore, TimeSpan TimeLimit)
{
    public MatchSettingsViewModel ToViewModel() => new(this);
}

/// <summary>
/// The settings for a bracket
/// </summary>
public sealed record BracketSettings(int WinningScore, TimeSpan TimeLimit, bool HasThirdPlaceMatch = true, bool IsFullAdvancement = true)
    : MatchSettings(WinningScore, TimeLimit)
{
    public new BracketSettingsViewModel ToViewModel() => new(this);
    public MatchSettingsViewModel ToMatchSettingsViewModel() => new(this);
}

/// <summary>
/// The settings for a tournament
/// </summary>
public sealed record Settings(
    DateTime? Date,
    MatchSettings PoolSettings,
    BracketSettings BracketSettings,
    Demographic Demographic,
    SkillLevel SkillLevel,
    bool ReyAllowed,
    bool RenAllowed,
    bool TanoAllowed,
    string? SubTitle
)
{
    public SettingsViewModel ToViewModel() => new(this);
}
