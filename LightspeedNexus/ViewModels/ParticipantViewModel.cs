using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LightspeedNexus.ViewModels;

public partial class ParticipantViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial ContestantViewModel Player { get; set; } = new();

    [ObservableProperty]
    public partial int MinorViolations { get; set; } = 0;

    [ObservableProperty]
    public partial int Score { get; set; } = 0;

    #endregion

    /// <summary>
    /// Creates a brand new participant
    /// </summary>
    public ParticipantViewModel() { }

    /// <summary>
    /// Loads a participant
    /// </summary>
    public ParticipantViewModel(Player participant, IReadOnlyList<ContestantViewModel> players)
    {
        MinorViolations = participant.MinorViolations;
        Player = players[participant.Fighter];
        Score = participant.Score;
    }

    /// <summary>
    /// Converts to a <see cref="Models.Player"/>.
    /// </summary>
    public Player ToModel(IList<ContestantViewModel> players) => new()
    {
        Fighter = players.IndexOf(Player),
        MinorViolations = MinorViolations,
        Score = Score,
    };
}
