using Lightspeed.Services;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Transitions;

public class SeedToBracketTransition(IServiceProvider serviceProvider, IActiveTournamentService activeTournamentService) : IStageTransition
{
    public StageViewModel GenerateNextStage(StageViewModel currentStage)
    {
        if (currentStage is not SeedingStageViewModel seedStage)
            throw new ArgumentException($"Expected {nameof(SeedingStageViewModel)}, got {currentStage.GetType().Name}");

        var vm = serviceProvider.GetRequiredService<BracketStageViewModel>();

        // build the list of participants, padded with nulls to handle byes
        int playerCount = seedStage.Seeds.Count;
        int bracketCount = BracketStage.FindBracketCount(playerCount, seedStage.IsFullAdvancement);
        var list = BuildList(seedStage.Seeds.Select(s => s.Participant), bracketCount);

        // load the initial round and wire up subsequent rounds to winners
        var startGroup = SetInitialRoundTo(vm, bracketCount, list);
        var group = startGroup;
        while (group.Matches.Count > 2)
            group = SetNextRoundFor(vm, group);
        vm.Final.NewEmptyMatch<StandardMatchViewModel>();
        vm.Third.NewEmptyMatch<StandardMatchViewModel>();

        // check for byes in the first round to automatically advance winners
        foreach (var match in startGroup.Matches)
            match.CheckByeWinner();

        vm.IsRanked = activeTournamentService.IsRanked;

        return vm;
    }

    /// <summary>
    /// Converts rankings into a padded list of rankings to match the given count. The padded items
    /// are null. This is useful in handling byes
    /// </summary>
    private static List<ParticipantViewModel?> BuildList(IEnumerable<ParticipantViewModel> rankings, int count)
    {
        List<ParticipantViewModel?> list = [];
        foreach (var seed in rankings)
            list.Add(seed);
        while (list.Count < count)
            list.Add(null);
        return list;
    }

    /// <summary>
    /// Sets the initial round identified by the number of matches
    /// </summary>
    private static MatchGroupViewModel SetInitialRoundTo(BracketStageViewModel vm, int count, List<ParticipantViewModel?> list)
    {
        var group = vm.FindBracketGroup(count);
        (int, int)[] loadout = BracketStage.Loadouts[count];
        for (int i = 0; i < loadout.Length; i++)
        {
            group.NewMatch<StandardMatchViewModel>(
                list[loadout[i].Item1], loadout[i].Item1 + 1,
                list[loadout[i].Item2], loadout[i].Item2 + 1
            );
        }
        return group;
    }

    /// <summary>
    /// Sets the next round by wiring it up to the given round's winners and returns it.
    /// This assumes that the given round has already been populated with matches and players, and that the next round is empty. It also assumes
    /// that the given round has an even number of matches, which should always be the case in a standard bracket.
    /// If any of these assumptions are violated, the behavior is undefined.
    /// </summary>
    private static MatchGroupViewModel SetNextRoundFor(BracketStageViewModel vm, MatchGroupViewModel previousRound)
    {
        // matches always represent two players, so if we pass the size of the previous
        // group (aka, number of matches), we'll have the correct amount of players for this
        // round, e.g. 8 matches in the previous round would be 8 players in the new one
        var group = vm.FindBracketGroup(previousRound.Matches.Count);
        for (int i = 0; (i + 1) < previousRound.Matches.Count; i += 2)
            group.NewEmptyMatch<StandardMatchViewModel>();

        return group;
    }
}
