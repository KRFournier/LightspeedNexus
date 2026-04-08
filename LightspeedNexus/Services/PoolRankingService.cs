using Lightspeed.ViewModels;
using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Services;

/// <summary>
/// This service calculates the ranks of participants based on their performance in a given pool
/// </summary>
public class PoolRankingService
{
    /// <summary>
    /// A participant's matches and statistics
    /// </summary>
    private class ParticipantStatistics(ParticipantViewModel participant)
    {
        public ParticipantViewModel Participant = participant;
        public List<(ParticipantViewModel Opponent, int PlayerScore, int OpponentScore, bool Winner)> Matches = [];
        public int Wins => Matches.Count(s => s.Winner);
        public int Losses => Matches.Count(s => !s.Winner);
        public int PointsWon => Matches.Sum(s => s.PlayerScore);
        public int PointsLost => Matches.Sum(s => s.OpponentScore);
        public double Score { get; private set; } = 0.0;

        public double ComputeScore(double winValue, double maxScorePerPlayer)
        {
            Score = (Wins * winValue + (PointsWon - PointsLost)) / maxScorePerPlayer * 100.0;
            return Score;
        }

        public double ComputeAdjustedScore(double winValue, double maxScorePerPlayer, double maxPointsPerMatch, HashSet<ParticipantViewModel> overOutliers, HashSet<ParticipantViewModel> underOutliers)
        {
            if (!overOutliers.Contains(Participant) && !underOutliers.Contains(Participant))
            {
                int wins = overOutliers.Count > 0 || Wins == 0 ? Wins : Wins - underOutliers.Count;
                int adjDiff = Matches.Sum(s => overOutliers.Contains(s.Opponent) || underOutliers.Contains(s.Opponent) ? 0 : s.PlayerScore - s.OpponentScore);
                Score = (wins * winValue + adjDiff) / (maxScorePerPlayer - (winValue + maxPointsPerMatch) * underOutliers.Count) * 100.0;
            }
            return Score;
        }
    }

    /// <summary>
    /// Calculates the ranking scores for just this pool
    /// </summary>
    public IEnumerable<(ParticipantViewModel Participant, int Wins, int Losses, int Points, int PointsAgainst, double Score)>
        CalculateScores(PoolViewModel pool)
    {
        // This is the furthest from the mean score a player can get before they are
        // removed from the calculation.
        const double deviationThreshold = 1.5;

        // The rankings are based on a "score" (not to be confused with points in a match)
        // Each match is given a win value of twice the maximum points possible.
        // The score is ultimately based on the percentage of total points possible.
        double matchesPerPlayer = pool.Squadron.Participants.Count - 1;
        double maxPointsPerMatch = pool.MatchGroup.Settings.WinningScore + PointValues.Max - 1.0;
        double winValue = maxPointsPerMatch * 2.0;
        double maxScorePerPlayer = (winValue + maxPointsPerMatch) * matchesPerPlayer;

        // gather statistics for each player
        Dictionary<ParticipantViewModel, ParticipantStatistics> stats = new([.. pool.Squadron.Participants
            .Select(p => new KeyValuePair<ParticipantViewModel, ParticipantStatistics>(p, new(p)))]);
        foreach (var match in pool.MatchGroup.Matches.Where(m => m.IsMatchCompleted))
        {
            if (match is StandardMatchViewModel stdMatch && stdMatch.First is not null && stdMatch.Second is not null)
            {
                stats[stdMatch.First.Participant].Matches.Add(
                    (stdMatch.Second.Participant, stdMatch.First.Points, stdMatch.Second.Points, stdMatch.IsFirstWinner));
                stats[stdMatch.Second.Participant].Matches.Add(
                    (stdMatch.First.Participant, stdMatch.Second.Points, stdMatch.First.Points, stdMatch.IsSecondWinner));
            }
        }

        // calculate the mean and standard deviation
        var (stdDev, mean) = StdDevAndMean(stats.Values.Select(s => (double)s.ComputeScore(winValue, maxScorePerPlayer)));
        stdDev *= deviationThreshold;

        // determine which players are outliers
        HashSet<ParticipantViewModel> overOutliers = [];
        HashSet<ParticipantViewModel> underOutliers = [];
        foreach (var stat in stats.Values)
        {
            if (stat.Score > mean + stdDev)
                overOutliers.Add(stat.Participant);
            else if (stat.Score < mean - stdDev)
                underOutliers.Add(stat.Participant);
        }

        return stats.Values.Select(
            s => (s.Participant,
                  s.Wins,
                  s.Losses,
                  s.PointsWon,
                  s.PointsLost,
                  s.ComputeAdjustedScore(winValue, maxScorePerPlayer, maxPointsPerMatch, overOutliers, underOutliers)
            )
        );
    }

    /// <summary>
    /// Computes statistical mean and standard deviation
    /// </summary>
    private static (double, double) StdDevAndMean(IEnumerable<double> values)
    {
        double mean = 0.0;
        double sum = 0.0;
        double stdDev = 0.0;
        int n = 0;
        foreach (double val in values)
        {
            n++;
            double delta = val - mean;
            mean += delta / n;
            sum += delta * (val - mean);
        }
        if (n > 0)
            stdDev = Math.Sqrt(sum / n);

        return (stdDev, mean);
    }
}
