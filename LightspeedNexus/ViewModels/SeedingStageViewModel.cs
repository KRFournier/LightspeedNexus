using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using LightspeedNexus.Transitions;
using System.Collections.ObjectModel;

namespace LightspeedNexus.ViewModels;

public partial class SeedViewModel : ObservableObject
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
    public partial int Points { get; set; } = 0;

    [ObservableProperty]
    public partial int PointsAgainst { get; set; } = 0;

    [ObservableProperty]
    public partial double Score { get; set; } = 0.0;

    #endregion

    public Seed ToModel() => new()
    {
        Place = Place,
        Participant = Participant.Guid,
        Wins = Wins,
        Losses = Losses,
        Points = Points,
        PointsAgainst = PointsAgainst,
        Score = Score
    };
}

public partial class SeedingStageViewModel(IServiceProvider serviceProvider, IMessenger messenger, NavigationService navigationService)
    : StageViewModel(serviceProvider, messenger, navigationService)
{
    #region Properties

    public override string Name => "Seeds";

    public ObservableCollection<SeedViewModel> Seeds { get; set; } = [];

    [ObservableProperty]
    public partial bool IsFullAdvancement { get; set; } = true;

    #endregion

    public override SeedingStage ToModel() => new()
    {
        IsFullAdvancement = IsFullAdvancement,
        Seeds = [.. Seeds.Select(s => s.ToModel())],
        Next = Next?.ToModel()
    };

    /// <summary>
    /// The seed stage always goes to the bracket stage
    /// </summary>
    public override IStageTransition GetTransitionToNextStage() => New<SeedToBracketTransition>();
}
