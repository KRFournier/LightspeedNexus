using LightspeedNexus.Services;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Transitions;

public class PoolsToSeedTransition(IServiceProvider serviceProvider, PoolRankingService poolRankingService) : IStageTransition
{
    public StageViewModel GenerateNextStage(StageViewModel currentStage)
    {
        if (currentStage is PoolsStageViewModel poolsStage)
        {
            var seedingStage = serviceProvider.GetRequiredService<SeedingStageViewModel>();

            var rankings = new List<SeedViewModel>();
            foreach (var pool in poolsStage.Pools)
            {
                foreach (var ranking in poolRankingService.CalculateScores(pool))
                {
                    var seed = serviceProvider.GetRequiredService<SeedViewModel>();
                    seed.Participant = ranking.Participant;
                    seed.Wins = ranking.Wins;
                    seed.Losses = ranking.Losses;
                    seed.Points = ranking.Points;
                    seed.PointsAgainst = ranking.PointsAgainst;
                    seed.Score = ranking.Score;
                    rankings.Add(seed);
                }
            }

            int i = 1;
            SeedViewModel? prev = null;
            foreach (var r in rankings.OrderByDescending(r => r.Wins).ThenByDescending(r => r.Score))
            {
                if (prev is not null && r.Wins == prev.Wins && r.Score == prev.Score)
                    r.Place = prev.Place;
                else
                    r.Place = i++;
                prev = r;
                seedingStage.Seeds.Add(r);
            }

            return seedingStage;
        }
        else
            throw new InvalidOperationException("Current stage must be PoolsStageViewModel.");
    }
}
