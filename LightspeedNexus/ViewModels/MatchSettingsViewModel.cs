using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LightspeedNexus.Models;
using System;
using System.Text.Json.Nodes;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// Settings for a group of matches
/// </summary>
public partial class MatchSettingsViewModel : ViewModelBase, IEquatable<MatchSettingsViewModel>
{
    #region Properties

    /// <summary>
    /// The points needed to win a match in this pool
    /// </summary>
    [ObservableProperty]
    public partial int WinningScore { get; set; } = 12;

    /// <summary>
    /// The time limit for matches in this pool
    /// </summary>
    [ObservableProperty]
    public partial TimeSpan TimeLimit { get; set; } = TimeSpan.FromSeconds(90);

    /// <summary>
    /// The number of rounds into which a match is divided
    /// </summary>
    [ObservableProperty]
    public partial int Rounds { get; set; } = 1;

    #endregion

    #region Commands

    [RelayCommand]
    private void DecreaseAll()
    {
        int chunk = WinningScore / 4;
        if (chunk-- > 0)
        {
            WinningScore = chunk * 4;
            TimeLimit = TimeSpan.FromSeconds(chunk * 30);
        }
    }

    [RelayCommand]
    private void IncreaseAll()
    {
        int chunk = WinningScore / 4 + 1;
        WinningScore = chunk * 4;
        TimeLimit = TimeSpan.FromSeconds(chunk * 30);
    }

    [RelayCommand]
    private void IncreaseScore() => WinningScore++;

    [RelayCommand]
    private void DecreaseScore()
    {
        if(WinningScore > 0)
            WinningScore--;
    }

    [RelayCommand]
    private void IncreaseTime() => TimeLimit += TimeSpan.FromSeconds(15);

    [RelayCommand]
    private void DecreaseTime()
    {
        if (TimeLimit.TotalSeconds > 15)
            TimeLimit -= TimeSpan.FromSeconds(15);
    }

    [RelayCommand]
    private void IncreaseRounds() => Rounds++;

    [RelayCommand]
    private void DecreaseRounds()
    {
        if (Rounds > 1)
            Rounds--;
    }

    #endregion

    /// <summary>
    /// Initializes the settings
    /// </summary>
    public MatchSettingsViewModel() { }

    /// <summary>
    /// Loads settings from a model
    /// </summary>
    public MatchSettingsViewModel(MatchSettings model)
    {
        WinningScore = model.WinningScore;
        TimeLimit = model.TimeLimit;
        Rounds = model.Rounds;
    }

    /// <summary>
    /// Creates a copy of the MatchSettingsViewModel
    /// </summary>
    public MatchSettingsViewModel Clone() => new()
    {
        WinningScore = WinningScore,
        TimeLimit = TimeLimit,
        Rounds = Rounds
    };

    /// <summary>
    /// The model
    /// </summary>
    public MatchSettings ToModel() => new(WinningScore, TimeLimit, Rounds);

    #region Value Equality

    public bool Equals(MatchSettingsViewModel? other) =>
        other is not null &&
        WinningScore == other.WinningScore &&
        TimeLimit == other.TimeLimit;

    public override bool Equals(object? obj) => Equals(obj as MatchSettingsViewModel);

    public override int GetHashCode() => HashCode.Combine(WinningScore, TimeLimit);

    public static bool operator ==(MatchSettingsViewModel? left, MatchSettingsViewModel? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(MatchSettingsViewModel? left, MatchSettingsViewModel? right) => !(left == right);

    #endregion

    #region Saber Sports

    /// <summary>
    /// Creates the json for submitting the tournament to saber-sports
    /// </summary>
    public JsonNode ToSaberSportsSubmission(string name, bool disablePromotion) => new JsonObject
    {
        ["name"] = name,
        ["duration"] = TimeLimit.ToString(),
        ["score"] = WinningScore,
        ["disable_rank_promotion"] = disablePromotion,
    };

    #endregion
}
