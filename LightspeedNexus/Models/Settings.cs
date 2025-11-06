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
public class MatchSettings
{
    public int WinningScore { get; set; } = 12;
    public TimeSpan TimeLimit { get; set; } = TimeSpan.FromSeconds(90);
    public MatchSettingsViewModel ToViewModel() => new(this);
    public MatchSettings() { }
    public MatchSettings(int winningScore, TimeSpan timeLimit)
    {
        WinningScore = winningScore;
        TimeLimit = timeLimit;
    }
}

/// <summary>
/// The settings for a bracket
/// </summary>
public sealed class BracketSettings : MatchSettings
{
    public bool HasThirdPlaceMatch { get; set; } = true;
    public bool IsFullAdvancement { get; set; } = true;
    public new BracketSettingsViewModel ToViewModel() => new(this);
    public MatchSettingsViewModel ToMatchSettingsViewModel() => new(this);
    public BracketSettings() { }
    public BracketSettings(int winningScore, TimeSpan timeLimit, bool hasThirdPlaceMatch, bool isFullAdvancement)
        : base(winningScore, timeLimit)
    {
        HasThirdPlaceMatch = hasThirdPlaceMatch;
        IsFullAdvancement = isFullAdvancement;
    }
}

/// <summary>
/// The settings for a tournament
/// </summary>
public sealed class Settings
{
    public DateTime? Date { get; set; }
    public MatchSettings PoolSettings { get; set; } = new();
    public BracketSettings BracketSettings { get; set; } = new();
    public Demographic Demographic { get; set; } = Demographic.All;
    public SkillLevel SkillLevel { get; set; } = SkillLevel.Open;
    public bool ReyAllowed { get; set; } = true;
    public bool RenAllowed { get; set; } = false;
    public bool TanoAllowed { get; set; } = false;
    public string? SubTitle { get; set; }
    public SettingsViewModel ToViewModel() => new(this);
    
    public Settings() { }
    public Settings(DateTime? date, MatchSettings poolSettings, BracketSettings bracketSettings, Demographic demographic, SkillLevel skillLevel, bool reyAllowed, bool renAllowed, bool tanoAllowed, string? subTitle)
    {
        Date = date;
        PoolSettings = poolSettings;
        BracketSettings = bracketSettings;
        Demographic = demographic;
        SkillLevel = skillLevel;
        ReyAllowed = reyAllowed;
        RenAllowed = renAllowed;
        TanoAllowed = tanoAllowed;
        SubTitle = subTitle;
    }
}
