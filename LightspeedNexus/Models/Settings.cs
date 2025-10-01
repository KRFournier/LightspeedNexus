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
/// The settings for a tournament
/// </summary>
public class Settings
{
    /// <summary>
    /// When the tournament takes place
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// The match settings used for pools.
    /// </summary>
    public MatchSettings PoolSettings { get; set; } = new();

    /// <summary>
    /// The match settings used for brackets.
    /// </summary>
    public BracketSettings BracketSettings { get; set; } = new();

    /// <summary>
    /// The demographic of the tournament
    /// </summary>
    public Demographic Demographic { get; set; } = Demographic.All;

    /// <summary>
    /// The skill level of the tournament
    /// </summary>
    public SkillLevel SkillLevel { get; set; } = SkillLevel.Open;

    /// <summary>
    /// Determines whether Rey-style weapons are allowed
    /// </summary>
    public bool ReyAllowed { get; set; } = true;

    /// <summary>
    /// Determines whether Ren-style weapons are allowed
    /// </summary>
    public bool RenAllowed { get; set; } = false;

    /// <summary>
    /// Determines whether Tano-style weapons are allowed
    /// </summary>
    public bool TanoAllowed { get; set; } = false;

    /// <summary>
    /// An optional tag to distinguish this tournament from others
    /// </summary>
    public string? SubTitle { get; set; } = null;
}
