using Avalonia.Controls;
using Avalonia.Media;
using Lightspeed.Services;
using LightspeedNexus.Models;
using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Services;

/// <summary>
/// Creates new view models from models loaded from the database.
/// </summary>
public class LoadingService(SharedLoadingService sharedLoadingService, StorageService storageService, SquadronService squadronService)
{
    private bool _isRanked = false;

    private readonly Dictionary<Guid, SquadronViewModel> _squadrons = [];

    public SquadronViewModel FindSquadron(Guid id)
    {
        if (_squadrons.TryGetValue(id, out var squadron))
            return squadron;
        throw new InvalidOperationException($"Squadron with id {id} not found.");
    }

    /// <summary>
    /// Loads a stage from a stage model. The type of stage is determined by the type of the model,
    /// and the appropriate loading method is called to create the view model.
    /// </summary>
    protected StageViewModel? LoadStage(Stage? model)
    {
        StageViewModel? stage = model switch
        {
            SetupStage ss => LoadSetupStage(ss),
            SquadronsStage sqs => LoadSquadronsStage(sqs),
            PoolsStage ps => NewPoolsStage(ps),
            SeedingStage sds => LoadSeedingStage(sds),
            BracketStage bs => LoadBracketStage(bs),
            ResultsStage rs => LoadResultsStage(rs),
            null => null,
            _ => throw new NotSupportedException("Unsupported stage type"),
        };

        return stage;
    }

    /// <summary>
    /// Loads a Bracket Stage from a model.
    /// </summary>
    protected BracketStageViewModel LoadBracketStage(BracketStage model)
    {
        var vm = sharedLoadingService.New<BracketStageViewModel>();
        vm.Top64 = LoadMatchGroup(model.Top64);
        vm.Top32 = LoadMatchGroup(model.Top32);
        vm.Top16 = LoadMatchGroup(model.Top16);
        vm.Quarterfinals = LoadMatchGroup(model.Quarterfinals);
        vm.Semifinals = LoadMatchGroup(model.Semifinals);
        vm.Third = LoadMatchGroup(model.Third);
        vm.Final = LoadMatchGroup(model.Final);
        vm.IsRanked = _isRanked;
        return vm;
    }

    /// <summary>
    /// Loads a fighter view model from a fighter model.
    /// </summary>
    public FighterViewModel LoadFighter(Fighter model)
    {
        var vm = sharedLoadingService.New<FighterViewModel>();
        vm.Guid = model.Id;
        vm.OnlineId = model.OnlineId;
        vm.FirstName = model.FirstName;
        vm.LastName = model.LastName;
        vm.Club = model.Club;
        vm.ReyRank = model.Rey;
        vm.RenRank = model.Ren;
        vm.TanoRank = model.Tano;
        return vm;
    }

    /// <summary>
    /// Loads a match group from the given model
    /// </summary>
    protected MatchGroupViewModel LoadMatchGroup(MatchGroup model)
    {
        var vm = sharedLoadingService.New<MatchGroupViewModel>();
        vm.Guid = model.Id;
        vm.Settings = sharedLoadingService.LoadMatchSettings(model.Settings);

        // load matches
        foreach (var match in model.Matches)
        {
            var newmatch = sharedLoadingService.LoadMatch(storageService.GetMatch(match));
            newmatch.Settings = vm.Settings;
            vm.Add(newmatch);
        }

        return vm;
    }

    /// <summary>
    /// Loads a new pool view model from the model
    /// </summary>
    protected PoolViewModel LoadPool(Pool model)
    {
        var vm = sharedLoadingService.New<PoolViewModel>();
        vm.Squadron = FindSquadron(model.Squadron);
        vm.MatchGroup = LoadMatchGroup(model.MatchGroup);
        return vm;
    }

    /// <summary>
    /// Loads a new pools stage from a <see cref="PoolsStage"/> model.
    /// </summary>
    protected PoolsStageViewModel NewPoolsStage(PoolsStage model)
    {
        var stage = sharedLoadingService.New<PoolsStageViewModel>();
        foreach (var pool in model.Pools)
            stage.AddPool(LoadPool(pool));
        return stage;
    }

    /// <summary>
    /// Loads a new registree view model from a <see cref="Registree"/> model.
    /// </summary>
    protected RegistreeViewModel LoadRegistree(Registree model)
    {
        var vm = sharedLoadingService.New<RegistreeViewModel>();
        vm.Guid = model.Id;
        vm.OnlineId = model.OnlineId;
        vm.FirstName = model.FirstName;
        vm.LastName = model.LastName;
        vm.Club = model.Club;
        vm.ReyRank = model.Rey;
        vm.RenRank = model.Ren;
        vm.TanoRank = model.Tano;
        vm.UseEffectiveRank = model.UsesEffectiveRank;
        vm.WeaponOfChoice = model.WeaponOfChoice;
        return vm;
    }

    /// <summary>
    /// Loads a new results stage view model from the given model. This is used to display the results of a tournament after it has completed.
    /// </summary>
    protected ResultsStageViewModel LoadResultsStage(ResultsStage model)
    {
        var vm = sharedLoadingService.New<ResultsStageViewModel>();
        vm.Placements = [.. model.Placements.Select(s => LoadStatistics(s))];
        vm.CanSubmit = model.CanSubmit;
        return vm;
    }

    /// <summary>
    /// Loads a new seeding stage view model from a <see cref="SeedingStage"/> model.
    /// </summary>
    protected SeedingStageViewModel LoadSeedingStage(SeedingStage model)
    {
        var vm = sharedLoadingService.New<SeedingStageViewModel>();
        vm.IsFullAdvancement = model.IsFullAdvancement;
        vm.Seeds = [.. model.Seeds.Select(s => LoadSeed(s))];
        return vm;
    }

    /// <summary>
    /// Loads a new seed view model from a <see cref="Seed"/> model.
    /// </summary>
    protected SeedViewModel LoadSeed(Seed model)
    {
        var vm = sharedLoadingService.New<SeedViewModel>();
        vm.Place = model.Place;
        vm.Participant = sharedLoadingService.FindParticipant(model.Participant);
        vm.Wins = model.Wins;
        vm.Losses = model.Losses;
        vm.Points = model.Points;
        vm.PointsAgainst = model.PointsAgainst;
        vm.Score = model.Score;
        return vm;
    }

    /// <summary>
    /// Loads a new setup stage view model from a <see cref="SetupStage"/> model.
    /// </summary>
    protected SetupStageViewModel LoadSetupStage(SetupStage model)
    {
        var vm = sharedLoadingService.New<SetupStageViewModel>();
        vm.Date = model.Date;
        vm.GameMode = model.GameMode;
        vm.Demographic = model.Demographic;
        vm.SkillLevel = model.SkillLevel;
        vm.ReyAllowed = model.ReyAllowed;
        vm.RenAllowed = model.RenAllowed;
        vm.TanoAllowed = model.TanoAllowed;
        vm.EventName = model.Event;
        vm.SubTitle = model.SubTitle;
        vm.AllowARanks = model.AllowARanks;
        vm.AllowBRanks = model.AllowBRanks;
        vm.AllowCRanks = model.AllowCRanks;
        vm.AllowDRanks = model.AllowDRanks;
        vm.AllowERanks = model.AllowERanks;
        vm.AllowURanks = model.AllowURanks;
        vm.Rings = [.. model.Rings];

        foreach (var r in model.Registrees)
            vm.AddRegistree(LoadRegistree(r));

        vm.ValidateRoster();

        return vm;
    }

    /// <summary>
    /// Loads a new squadron view model from a <see cref="Squadron"/> model.
    /// </summary>
    protected SquadronViewModel LoadSquadron(Squadron model)
    {
        var vm = sharedLoadingService.New<SquadronViewModel>();
        vm.Guid = model.Guid;
        vm.Participants = [.. model.Participants.Select(id => sharedLoadingService.FindParticipant(id))];
        vm.Weight = model.Weight;
        vm.Settings = sharedLoadingService.LoadMatchSettings(model.MatchSettings);
        _squadrons.Add(vm.Guid, vm);
        return vm;
    }

    /// <summary>
    /// Loads squadrons stage from a <see cref="SquadronsStage"/> model.
    /// </summary>
    protected SquadronsStageViewModel LoadSquadronsStage(SquadronsStage model)
    {
        var vm = sharedLoadingService.New<SquadronsStageViewModel>();
        vm.IsAutoAssigned = model.IsAutoAssigned;
        vm.Participants = [.. model.Participants.Select(p => sharedLoadingService.LoadParticipant(p))];

        int i = 0;
        vm.Squadrons = [.. model.Squadrons.Select(s =>
        {
            var svm = LoadSquadron(s);
            var (name, color) = squadronService.GetSquadronDefinition(i);
            svm.Name = name;
            svm.Color = App.Current?.FindResource($"{color}Brush") as IBrush ?? Brushes.Transparent;
            i++;
            return svm;
        })];

        return vm;
    }

    /// <summary>
    /// Loads a new statistics view model from a <see cref="Statistics"/> model.
    /// </summary>
    protected StatisticsViewModel LoadStatistics(Statistics model)
    {
        var vm = sharedLoadingService.New<StatisticsViewModel>();
        vm.Place = model.Place;
        vm.Participant = sharedLoadingService.FindParticipant(model.Participant);
        vm.Wins = model.Wins;
        vm.Points = model.Points;
        vm.PossiblePoints = model.PossiblePoints;
        vm.Value = model.Value;
        vm.OldRank = model.OldRank;
        vm.NewRank = model.NewRank;
        return vm;
    }

    /// <summary>
    /// Loads a new tournament view model from a <see cref="Tournament"/> model.
    /// </summary>
    public TournamentViewModel LoadTournament(Tournament model)
    {
        var vm = sharedLoadingService.New<TournamentViewModel>();

        vm.Guid = model.Id;

        // load the setup stage
        vm.SetupStage = LoadSetupStage(model.SetupStage);
        _isRanked = vm.IsRanked;

        // load the other stages. We do this in a loop to avoid recursion issues with the stages referencing each other.
        // Since the stages are loaded in bulk, we want to ensure that later stages have access to the view models of earlier stages.
        StageViewModel? currentStageViewModel = vm.SetupStage;
        StageViewModel? prevStage = null;
        var nextStage = model.SetupStage.Next;
        while (currentStageViewModel is not null)
        {
            currentStageViewModel.Previous = prevStage;
            currentStageViewModel.Next = LoadStage(nextStage);
            prevStage = currentStageViewModel;

            currentStageViewModel = currentStageViewModel.Next;
            nextStage = nextStage?.Next;
        }

        return vm;
    }
}
