using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LightspeedNexus.ViewModels;

public partial class PoolViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial SquadronViewModel Squadron { get; protected set; }

    public ObservableCollection<PoolMatchViewModel> Matches { get; set; } = [];

    #endregion

    public PoolViewModel(SquadronViewModel squadron)
    {
        Squadron = squadron;
    }

    public PoolViewModel(Pool pool, IReadOnlyList<SquadronViewModel> squadrons, IReadOnlyList<ContestantViewModel> fullRoster)
    {
        Squadron = squadrons[pool.Squadron];
        foreach (var match in pool.Matches)
        {
            Matches.Add(new PoolMatchViewModel(Squadron, match, fullRoster));
        }
    }

    public Pool ToModel(IList<SquadronViewModel> squadrons, IList<ContestantViewModel> fullRoster) =>
        new(squadrons.IndexOf(Squadron), [.. Matches.Select(m => m.ToModel(fullRoster))]);

    #region Statistics

    /// <summary>
    /// A match from a player's perspective
    /// </summary>
    internal record PlayerMatchStatistics(ContestantViewModel Opponent, int PlayerScore, int OpponentScore, bool Winner);

    /// <summary>
    /// A player's matches and statistics
    /// </summary>
    internal class PlayerStatistics(ContestantViewModel player)
    {
        public ContestantViewModel Player = player;
        public List<PlayerMatchStatistics> Matches = [];
        public int Wins => Matches.Count(s => s.Winner);
        public int Points => Matches.Sum(s => s.PlayerScore);
        public int OpponentPoints => Matches.Sum(s => s.OpponentScore);
        public double Score { get; private set; } = 0.0;

        public double ComputeScore(double winValue, double maxScorePerPlayer)
        {
            Score = (Wins * winValue + (Points - OpponentPoints)) / maxScorePerPlayer * 100.0;
            return Score;
        }

        public double ComputeAdjustedScore(double winValue, double maxScorePerPlayer, double maxPointsPerMatch, HashSet<ContestantViewModel> overOutliers, HashSet<ContestantViewModel> underOutliers)
        {
            if (!overOutliers.Contains(Player) && !underOutliers.Contains(Player))
            {
                int wins = overOutliers.Count > 0 || Wins == 0 ? Wins : Wins - underOutliers.Count;
                int adjDiff = Matches.Sum(s => overOutliers.Contains(s.Opponent) || underOutliers.Contains(s.Opponent) ? 0 : s.PlayerScore - s.OpponentScore);
                Score = (wins * winValue + adjDiff) / (maxScorePerPlayer - (winValue + maxPointsPerMatch) * underOutliers.Count) * 100.0;
            }
            return Score;
        }
    }

    #endregion

    #region Match Arrangement

    /// <summary>
    /// Supports pools of 2 to 20 players
    /// </summary>
    static readonly int[][][] poolArrangements = [
        [[]],
        [[]],
        [[1, 2]],
        [[2, 3], [1, 3], [1, 2]],
        [[1, 4], [2, 3], [1, 3], [2, 4], [3, 4], [1, 2]],
        [[1, 2], [3, 4], [5, 1], [2, 3], [5, 4], [1, 3], [2, 5], [4, 1], [3, 5], [4, 2]],
        [[1, 2], [4, 3], [6, 5], [3, 1], [2, 6], [5, 4], [1, 6], [3, 5], [4, 2], [5, 1], [6, 4], [2, 3], [1, 4], [5, 2], [3, 6]],
        [[1, 4], [2, 5], [3, 6], [7, 1], [5, 4], [2, 3], [6, 7], [5, 1], [4, 3], [6, 2], [5, 7], [3, 1], [4, 6], [7, 2], [3, 5], [1, 6], [2, 4], [7, 3], [6, 5], [1, 2], [4, 7]],
        [[2, 3], [1, 5], [7, 4], [6, 8], [1, 2], [3, 4], [5, 6], [8, 7], [4, 1], [5, 2], [8, 3], [6, 7], [4, 2], [8, 1], [7, 5], [3, 6], [2, 8], [5, 4], [6, 1], [3, 7], [4, 8], [2, 6], [3, 5], [1, 7], [4, 6], [8, 5], [7, 2], [1, 3]],
        [[1, 9], [2, 8], [3, 7], [4, 6], [1, 5], [2, 9], [8, 3], [7, 4], [6, 5], [1, 2], [9, 3], [8, 4], [7, 2], [6, 1], [3, 2], [9, 4], [5, 8], [7, 6], [3, 1], [2, 4], [5, 9], [8, 6], [7, 1], [4, 3], [5, 2], [6, 9], [8, 7], [4, 1], [5, 3], [6, 2], [9, 7], [1, 8], [4, 5], [3, 6], [5, 7], [9, 8]],
        [[1, 4], [6, 9], [2, 5], [7, 10], [3, 1], [8, 6], [4, 5], [9, 10], [2, 3], [7, 8], [5, 1], [10, 6], [4, 2], [9, 7], [5, 3], [10, 8], [1, 2], [6, 7], [3, 4], [8, 9], [5, 10], [1, 6], [2, 7], [3, 8], [4, 9], [6, 5], [10, 2], [8, 1], [7, 4], [9, 3], [2, 6], [5, 8], [4, 10], [1, 9], [3, 7], [8, 2], [6, 4], [9, 5], [10, 3], [7, 1], [4, 8], [2, 9], [3, 6], [5, 7], [1, 10]],
        [[1, 2], [7, 8], [4, 5], [10, 11], [2, 3], [8, 9], [5, 6], [3, 1], [9, 7], [6, 4], [2, 5], [8, 11], [1, 4], [7, 10], [5, 3], [11, 9], [1, 6], [4, 2], [10, 8], [3, 6], [5, 1], [11, 7], [3, 4], [9, 10], [6, 2], [1, 7], [3, 9], [10, 4], [8, 2], [5, 11], [1, 8], [9, 2], [3, 10], [4, 11], [6, 7], [9, 1], [2, 10], [11, 3], [7, 5], [6, 8], [10, 1], [11, 2], [4, 7], [8, 5], [6, 9], [11, 1], [7, 3], [4, 8], [9, 5], [6, 10], [2, 7], [8, 3], [4, 9], [10, 5], [6, 11]],
        [[1, 2], [3, 4], [5, 6], [7, 8], [9, 10], [11, 12], [3, 1], [2, 4], [7, 5], [6, 8], [11, 9], [10, 12], [4, 1], [2, 3], [8, 5], [6, 7], [12, 9], [10, 11], [1, 5], [4, 8], [6, 2], [7, 3], [9, 1], [12, 5], [4, 10], [8, 11], [2, 7], [3, 6], [5, 9], [1, 12], [8, 10], [11, 4], [5, 2], [9, 7], [12, 3], [1, 6], [10, 2], [5, 11], [8, 9], [4, 7], [3, 10], [12, 6], [11, 1], [2, 8], [9, 4], [7, 10], [5, 3], [6, 11], [2, 12], [1, 8], [4, 5], [9, 3], [7, 11], [10, 6], [8, 12], [9, 2], [7, 1], [6, 4], [3, 11], [10, 5], [12, 7], [6, 9], [8, 3], [1, 10], [4, 12], [11, 2]],
        [[1, 2], [3, 4], [5, 6], [7, 8], [9, 10], [11, 12], [13, 1], [2, 3], [4, 5], [6, 7], [8, 9], [10, 11], [12, 1], [2, 13], [3, 5], [4, 6], [7, 9], [8, 10], [1, 11], [12, 2], [13, 3], [5, 7], [9, 4], [6, 8], [10, 1], [11, 2], [3, 12], [5, 13], [7, 4], [9, 6], [1, 8], [2, 10], [11, 3], [12, 5], [4, 13], [1, 7], [6, 2], [3, 9], [8, 11], [10, 5], [4, 12], [13, 7], [6, 1], [2, 9], [8, 3], [5, 11], [10, 4], [7, 12], [13, 6], [9, 1], [2, 8], [11, 4], [3, 10], [12, 6], [9, 13], [1, 5], [7, 2], [4, 8], [6, 11], [10, 12], [1, 3], [8, 13], [5, 9], [11, 7], [6, 10], [12, 8], [2, 4], [13, 11], [3, 7], [8, 5], [9, 12], [10, 13], [4, 1], [3, 6], [5, 2], [11, 9], [7, 10], [12, 13]],
        [[1, 2], [3, 4], [5, 6], [7, 8], [9, 10], [11, 12], [13, 14], [3, 1], [2, 4], [7, 5], [6, 8], [11, 9], [10, 12], [1, 13], [14, 3], [2, 5], [4, 7], [6, 9], [8, 11], [10, 1], [12, 13], [3, 2], [5, 14], [4, 6], [9, 7], [8, 1], [11, 10], [12, 2], [13, 3], [4, 5], [14, 6], [1, 7], [8, 9], [2, 10], [11, 3], [12, 4], [5, 13], [6, 1], [7, 14], [2, 8], [9, 3], [10, 4], [12, 14], [5, 11], [13, 6], [7, 2], [1, 12], [14, 8], [3, 10], [4, 9], [1, 5], [6, 11], [12, 7], [2, 13], [8, 3], [10, 14], [4, 1], [9, 5], [6, 7], [11, 2], [3, 12], [13, 8], [14, 1], [5, 10], [11, 4], [2, 9], [3, 6], [7, 13], [8, 12], [1, 11], [4, 14], [5, 3], [10, 6], [9, 13], [14, 2], [7, 11], [8, 4], [12, 5], [1, 9], [13, 10], [6, 2], [3, 7], [9, 12], [14, 11], [13, 4], [5, 8], [10, 7], [12, 6], [11, 13], [9, 14], [8, 10]],
        ];

    private void ArrangeMatches()
    {
        if (Squadron.Players.Count > 1 && Squadron.Players.Count <= poolArrangements.Length)
        {
            Matches.Clear();
            foreach (var arrangement in poolArrangements[Squadron.Players.Count])
            {
                var match = new MatchViewModel(Squadron.Players[arrangement[0] - 1], Squadron.Players[arrangement[1] - 1]);
            }
        }
    }

    #endregion
}
