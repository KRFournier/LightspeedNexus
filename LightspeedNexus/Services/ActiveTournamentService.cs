using Lightspeed.Services;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Services;

/// <summary>
/// This service is used by the view models to get information about the currently active tournament.
/// It is also used to generate the JSON that is submitted to saber-sport.com when the tournament is completed.
/// </summary>
public class ActiveTournamentService(IServiceProvider serviceProvider, NavigationService navigationService) : IActiveTournamentService
{
    private IServiceScope? _activeScope;

    /// <summary>
    /// Starts a new tournament
    /// </summary>
    public void StartNewTournament()
    {
        _activeScope?.Dispose();
        _activeScope = serviceProvider.CreateScope();
        Active = _activeScope.ServiceProvider.GetRequiredService<TournamentViewModel>();
        navigationService.NavigateToTournament(Active);
    }

    /// <summary>
    /// Activates an existing tournament by loading it from the provided model and navigating to it.
    /// </summary>
    public void StartLoadedTournament(Tournament model)
    {
        _activeScope?.Dispose();
        _activeScope = serviceProvider.CreateScope();
        Active = _activeScope.ServiceProvider.GetRequiredService<LoadingService>().LoadTournament(model);
        navigationService.NavigateToTournament(Active);
    }

    /// <summary>
    /// The currently active tournament. This is set by the main view model when a tournament is brought to focus.
    /// </summary>
    public TournamentViewModel? Active { get; private set; }

    /// <summary>
    /// Determines if the weapon icons should be shown in the UI.
    /// </summary>
    public bool ShowingWeapons => Active is not null && new[]
    {
        Active.SetupStage.RenAllowed,
        Active.SetupStage.RenAllowed,
        Active.SetupStage.TanoAllowed
    }.Count(x => x) > 1;

    /// <summary>
    /// Determines if the tournament is ranked. A tournament is ranked if it is in standard game mode and has a rankable number of participants.
    /// </summary>
    public bool IsRanked => Active?.IsRanked ?? false;

    /// <summary>
    /// The tournament's value, which is determined by the number of participants and the game mode. This is used for ranking purposes on saber-sport.com.
    /// </summary>
    public int TournamentValue => Active?.Value ?? 0;

    /// <summary>
    /// The final grading of the tournament, which is determined by the ranks of the participants, the size of the tournament, and who made it to the top.
    /// </summary>
    public Grading? FinalGrading => Active?.GetFinalGrading();

    /// <summary>
    /// Determines if saber-sport.com submission is allowed for this tournament. This is true if the tournament is in standard game mode.
    /// </summary>
    public bool CanSubmit => Active?.SetupStage is not null && Active.SetupStage.GameMode == GameMode.Standard;

    /// <summary>
    /// The number of registrees
    /// </summary>
    public int RegistreeCount => Active?.SetupStage.Registrees.Count ?? 0;

    /// <summary>
    /// Determines if the registrees are permitted to choose a weapon
    /// </summary>
    public bool HasChoice => Active?.SetupStage.HasChoice ?? false;

    /// <summary>
    /// Gets the participants in the active tournament. If the active tournament is null or does not have a squadrons stage, returns an empty enumerable.
    /// </summary>
    public IEnumerable<ParticipantViewModel> Participants => Active?.FindStage<SquadronsStageViewModel>()?.Participants ?? Enumerable.Empty<ParticipantViewModel>();

    /// <summary>
    /// Serializes the current active saber score and returns it as a JSON string with the specified signature.
    /// </summary>
    /// <param name = "signature" > The signature to include in the serialized JSON output.Cannot be null.</param>
    /// <returns>A JSON string representing the active saber score with the provided signature. Returns an empty string if there
    /// is no active score.</returns>
    public string GetSaberScoreJson(string signature)
    {
        if (Active is not null)
        {
            var node = SaberSportsSerializer.Serialize(Active);
            node["signature"] = signature;
            return node.ToJsonString();
        }
        return string.Empty;
    }
}
