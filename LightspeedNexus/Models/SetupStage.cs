using System;
using System.Collections.Generic;
using System.Text;

namespace LightspeedNexus.Models;

/// <summary>
/// The tournament's game mode
/// </summary>
public enum GameMode
{
    Standard,
    Duo,
    Annihilation
}

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
public sealed class SetupStage : Stage
{
    public string? Event { get; set; }
    public DateTime? Date { get; set; }
    public GameMode GameMode { get; set; } = GameMode.Standard;
    public Demographic Demographic { get; set; } = Demographic.All;
    public SkillLevel SkillLevel { get; set; } = SkillLevel.Open;
    public bool ReyAllowed { get; set; } = true;
    public bool RenAllowed { get; set; } = false;
    public bool TanoAllowed { get; set; } = false;
    public string? SubTitle { get; set; }
    public Registree[] Registrees { get; set; } = [];
    public bool AllowARanks { get; set; } = true;
    public bool AllowBRanks { get; set; } = true;
    public bool AllowCRanks { get; set; } = true;
    public bool AllowDRanks { get; set; } = true;
    public bool AllowERanks { get; set; } = true;
    public bool AllowURanks { get; set; } = true;

    /// <summary>
    /// The name of the tournament, e.g., Open Rey
    /// </summary>
    public string Title => Tournament.GetTitle(Demographic,
        AllowARanks, AllowBRanks, AllowCRanks, AllowDRanks, AllowERanks, AllowURanks,
        GameMode, ReyAllowed, RenAllowed, TanoAllowed, SubTitle);
}
