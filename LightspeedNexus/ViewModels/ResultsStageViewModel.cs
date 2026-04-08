using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using LightspeedNexus.Transitions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LightspeedNexus.ViewModels;

public partial class ResultsStageViewModel(IServiceProvider serviceProvider, IMessenger messenger, NavigationService navigationService,
    SaberSportsService saberSportsService, ActiveTournamentService activeTournamentService)
    : StageViewModel(serviceProvider, messenger, navigationService)
{
    #region Properties

    public override string Name => "Results";

    public ObservableCollection<StatisticsViewModel> Placements { get; set; } = [];

    /// <summary>
    /// When a stage represents the final stage of a tournament, this will be true
    /// </summary>
    public override bool IsTournamentCompleted => true;

    [ObservableProperty]
    public partial bool CanSubmit { get; set; } = activeTournamentService.CanSubmit;

    #endregion

    #region Commands

    /// <summary>
    /// Submits the tournament to saber-sport.com
    /// </summary>
    [RelayCommand]
    private async Task Submit()
    {
        if (CanSubmit)
        {
            try
            {
                var signature = await ShowSignDialog();
                if (signature is null)
                    return;
                BeginWait("Submitting to Saber Sport...");
                string json = activeTournamentService.GetSaberScoreJson(signature);
                var (_, msg) = await saberSportsService.Submit(json);
                EndWait();
                await MessageBox(msg);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unexpected error editing a fighter: {e}");
            }
        }
    }

    #endregion

    public override ResultsStage ToModel() => new()
    {
        Placements = [.. Placements.Select(s => s.ToModel())],
        CanSubmit = CanSubmit,
        Next = Next?.ToModel()
    };

    public override IStageTransition GetTransitionToNextStage() => New<GoHomeTransition>();
}
