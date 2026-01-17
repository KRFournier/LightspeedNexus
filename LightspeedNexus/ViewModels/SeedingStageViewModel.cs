using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class SeedViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial int Place { get; set; } = 0;

    [ObservableProperty]
    public partial ParticipantViewModel Participant { get; set; }

    [ObservableProperty]
    public partial int Wins { get; set; } = 0;

    [ObservableProperty]
    public partial int Losses { get; set; } = 0;

    [ObservableProperty]
    public partial double Score { get; set; } = 0.0;

    #endregion

    public Seed ToModel() => new()
    {
        Place = Place,
        Participant = Participant.Guid,
        Wins = Wins,
        Losses = Losses,
        Score = Score
    };

    public static SeedViewModel FromModel(Seed model) => new()
    {
        Place = model.Place,
        Participant = StrongReferenceMessenger.Default.Send(new RequestParticipant(model.Participant)),
        Wins = model.Wins,
        Losses = model.Losses,
        Score = model.Score
    };
}

public partial class SeedingStageViewModel : StageViewModel
{
    #region Properties

    public ObservableCollection<SeedViewModel> Seeds { get; set; } = [];

    [ObservableProperty]
    public partial bool IsFullAdvancement { get; set; } = true;

    #endregion

    #region Commands

    [RelayCommand]
    private void GoToBracket() => Next = BracketStageViewModel.FromRankedList(Seeds.Select(s => s.Participant), IsFullAdvancement);

    #endregion

    public SeedingStageViewModel() : base("Seeds")
    {
    }

    public override SeedingStage ToModel() => new()
    {
        IsFullAdvancement = IsFullAdvancement,
        Seeds = [.. Seeds.Select(s => s.ToModel())],
        Next = Next?.ToModel()
    };

    public static SeedingStageViewModel FromModel(SeedingStage model) => new()
    {
        IsFullAdvancement = model.IsFullAdvancement,
        Seeds = [.. model.Seeds.Select(s => SeedViewModel.FromModel(s))],
        Next = FromModel(model.Next)
    };

    public static SeedingStageViewModel FromPools(PoolsStageViewModel poolsStage)
    {
        var stage = new SeedingStageViewModel();

        stage.Seeds.Clear();

        var rankings = new List<SeedViewModel>();
        foreach (var pool in poolsStage.Pools)
            foreach (var ranking in pool.CalculateScores(pool.MatchGroup.Settings.WinningScore, 5))
                rankings.Add(new SeedViewModel
                {
                    Participant = ranking.Participant,
                    Wins = ranking.Wins,
                    Losses = ranking.Losses,
                    Score = ranking.Score
                });

        int i = 1;
        SeedViewModel? prev = null;
        foreach (var r in rankings.OrderByDescending(r => r.Wins).ThenByDescending(r => r.Score))
        {
            if (prev is not null && r.Wins == prev.Wins && r.Score == prev.Score)
                r.Place = prev.Place;
            else
                r.Place = i++;
            prev = r;
            stage.Seeds.Add(r);
        }

        return stage;
    }
}
