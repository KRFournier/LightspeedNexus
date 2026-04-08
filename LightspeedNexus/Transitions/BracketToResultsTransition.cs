using Lightspeed.ViewModels;
using LightspeedNexus.Services;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Transitions;

public class BracketToResultsTransition(IServiceProvider serviceProvider, ActiveTournamentService activeTournamentService) : IStageTransition
{
    public StageViewModel GenerateNextStage(StageViewModel currentStage)
    {
        if (currentStage is not BracketStageViewModel bracketStage)
            throw new ArgumentException($"Expected {nameof(BracketStageViewModel)}, got {currentStage.GetType().Name}");

        var resultsStage = serviceProvider.GetRequiredService<ResultsStageViewModel>();

        var poolsStage = bracketStage.FindPreviousStage<PoolsStageViewModel>() ?? throw new InvalidOperationException("Pools stage not found");

        // initialize the dictionary with each player
        Dictionary<ParticipantViewModel, StatisticsViewModel> stats = [];
        int possibleWins = bracketStage.PossibleWins;
        foreach (var participant in activeTournamentService.Participants)
        {
            if (participant is not null)
            {
                var vm = serviceProvider.GetRequiredService<StatisticsViewModel>();
                vm.Place = GetPlace(bracketStage, participant);
                vm.OldRank = participant is PlayerViewModel p ? p.Rank : null;
                vm.Participant = participant;
                vm.PossiblePoints = (25 * possibleWins) + 100;
                stats[participant] = vm;
            }
        }

        // process each pool match
        foreach (var pool in poolsStage.Pools)
            foreach (var match in pool.MatchGroup.Matches)
                ProcessMatch(stats, match, activeTournamentService.TournamentValue);

        // process each bracket match
        foreach (var match in bracketStage.EnumerateMatches())
            ProcessMatch(stats, match, activeTournamentService.TournamentValue);
        // set the stats
        foreach (var stat in stats.Values.OrderBy(s => s.Place).ThenBy(s => s.Points))
            resultsStage.Placements.Add(stat);

        // upgrades
        if (activeTournamentService.FinalGrading is not null)
        {
            for (int i = 0; i < activeTournamentService.FinalGrading.Awards.Length && i < resultsStage.Placements.Count; i++)
                resultsStage.Placements[i].NewRank = activeTournamentService.FinalGrading.Awards[i];
        }

        return resultsStage;
    }

    /// <summary>
    /// Updates the stats for the given player
    /// </summary>
    private static void ProcessMatch(Dictionary<ParticipantViewModel, StatisticsViewModel> stats, MatchViewModel match, int value)
    {
        if (match.HasBye || match.IsEmpty)
            return;

        if (stats.TryGetValue(match.First.Participant, out StatisticsViewModel? firstStats))
            firstStats.AddMatch(value, match);
        if (stats.TryGetValue(match.Second.Participant, out StatisticsViewModel? secondStats))
            secondStats.AddMatch(value, match);
    }



    /// <summary>
    /// Gets a participant's final placing in the bracket
    /// </summary>
    private static int GetPlace(BracketStageViewModel bracket, ParticipantViewModel participant)
    {
        if (participant == bracket.Final[0].Winner?.Participant)
            return 1;
        else if (participant == bracket.Final[0].Loser?.Participant)
            return 2;
        else if ((bracket.Third[0].Winner is not null && participant == bracket.Third[0].Winner!.Participant) ||
                  (bracket.Third[0].Winner is null && bracket.Third[0].Contains(participant)))
            return 3;
        else if (bracket.Third[0].Loser is not null && participant == bracket.Third[0].Loser!.Participant)
            return 4;
        else if (bracket.Quarterfinals.Contains(participant))
            return 5;
        else if (bracket.Top16.Contains(participant))
            return 9;
        else if (bracket.Top32.Contains(participant))
            return 17;
        else return 33;
    }
}
