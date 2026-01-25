using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace LightspeedNexus.ViewModels;

public partial class PoolViewModel : ViewModelBase
{
    #region Properties

    /// <summary>
    /// The squadron assigned to this pool
    /// </summary>
    [ObservableProperty]
    public partial SquadronViewModel Squadron { get; protected set; } = new();

    /// <summary>
    /// Matches are view/viewmodel driven. We just reference them here, and the view takes
    /// care of the rest. This means all match viewmodels must have a corresponding view
    /// </summary>
    [ObservableProperty]
    public partial MatchGroupViewModel MatchGroup { get; set; } = new();

    #endregion

    /// <summary>
    /// Converts to a <see cref="Pool"/>
    /// </summary>
    public Pool ToModel() => new()
    {
        Squadron = Squadron.Guid,
        MatchGroup = MatchGroup.ToModel()
    };

    /// <summary>
    /// Converts from a <see cref="Pool"/>
    /// </summary>
    public static PoolViewModel FromModel(Pool pool) => new()
    {
        Squadron = StrongReferenceMessenger.Default.Send(new RequestSquadron(pool.Squadron)),
        MatchGroup = MatchGroupViewModel.FromModel(pool.MatchGroup)
    };

    /// <summary>
    /// Creates from a <see cref="SquadronViewModel"/>
    /// </summary>
    public static PoolViewModel FromSquadron(SquadronViewModel squadron)
    {
        PoolViewModel vm = new()
        {
            Squadron = squadron,
            MatchGroup = new() { Settings = squadron.Settings.Clone() }
        };

        // find the match arrangements for the number of participants, and create matches accordingly
        if (squadron.Participants.Count > 1 && squadron.Participants.Count <= poolArrangements.Length)
        {
            int i = 1;
            foreach (var arrangement in poolArrangements[squadron.Participants.Count])
                vm.MatchGroup.NewMatch<StandardMatchViewModel>(
                    vm.Squadron.Participants[arrangement[0] - 1],
                    vm.Squadron.Participants[arrangement[1] - 1],
                    i++
                );
        }

        return vm;
    }

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

    #endregion

    #region Statistics

    /// <summary>
    /// A match from a participants's perspective
    /// </summary>
    internal record PovMatchStatistics(ParticipantViewModel Opponent, int PlayerScore, int OpponentScore, bool Winner);

    /// <summary>
    /// A participant's matches and statistics
    /// </summary>
    internal class ParticipantStatistics(ParticipantViewModel participant)
    {
        public ParticipantViewModel Participant = participant;
        public List<PovMatchStatistics> Matches = [];
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
    public IEnumerable<(ParticipantViewModel Participant, int Wins, int Losses, int Points, int PointsAgainst, double Score)> CalculateScores(int winningScore, int maxSingleActionPoints)
    {
        // This is the furthest from the mean score a player can get before they are
        // removed from the calculation.
        const double deviationThreshold = 1.5;

        // The rankings are based on a "score" (not to be confused with points in a match)
        // Each match is given a win value of twice the maximum points possible.
        // The score is ultimately based on the percentage of total points possible.
        double matchesPerPlayer = Squadron.Participants.Count - 1;
        double maxPointsPerMatch = winningScore + maxSingleActionPoints - 1.0;
        double winValue = maxPointsPerMatch * 2.0;
        double maxScorePerPlayer = (winValue + maxPointsPerMatch) * matchesPerPlayer;

        // gather statistics for each player
        Dictionary<ParticipantViewModel, ParticipantStatistics> stats = new([.. Squadron.Participants
            .Select(p => new KeyValuePair<ParticipantViewModel, ParticipantStatistics>(p, new(p)))]);
        foreach (var match in MatchGroup.Matches .Where(m => m.IsMatchCompleted))
        {
            if (match is StandardMatchViewModel stdMatch && stdMatch.First is not null && stdMatch.Second is not null)
            {
                stats[stdMatch.First.Participant].Matches.Add(
                    new PovMatchStatistics(stdMatch.Second.Participant, stdMatch.First.Points, stdMatch.Second.Points, stdMatch.IsFirstWinner));
                stats[stdMatch.Second.Participant].Matches.Add(
                    new PovMatchStatistics(stdMatch.First.Participant, stdMatch.Second.Points, stdMatch.First.Points, stdMatch.IsSecondWinner));
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

    #endregion

    #region Saber Sports

    /// <summary>
    /// Creates the json for submitting the tournament to saber-sports
    /// </summary>
    public JsonNode ToSaberSportsSubmission(int index)
    {
        JsonArray participantsNode = [];
        JsonArray scores = [];
        JsonArray actions = [];

        var players = Squadron.Participants.OfType<PlayerViewModel>().ToList();

        // build the array of participants
        for (int i = 0; i < players.Count; i++)
        {
            participantsNode.Add(new JsonObject
            {
                ["uuid"] = players[i].SaberSportId,
                ["index"] = i
            });
            scores.Add(new JsonArray([.. Enumerable.Repeat<string?>(null, players.Count)]));
        }

        // create a 2D array of scores and populate them from the matches
        foreach (StandardMatchViewModel match in MatchGroup.Matches.OfType<StandardMatchViewModel>())
        {
            int blueIndex = players.IndexOf(match.First.Participant as PlayerViewModel ?? throw new InvalidOperationException("First participant is not a PlayerViewModel"));
            int redIndex = players.IndexOf(match.Second.Participant as PlayerViewModel ?? throw new InvalidOperationException("Second participant is not a PlayerViewModel"));
            if (blueIndex >= 0 && redIndex >= 0)
            {
                scores[blueIndex]![redIndex] = match.First.Points.ToString();
                scores[redIndex]![blueIndex] = match.Second.Points.ToString();

                actions.Add(new JsonArray([
                    new JsonObject
                    {
                        ["index"] = blueIndex,
                        ["actions"] = new JsonArray([.. match.FirstActions.ToSaberScore()]),
                        ["clock"] = match.Clock.Timer.ToString("m\\:ss")
                    },
                    new JsonObject
                    {
                        ["index"] = redIndex,
                        ["actions"] = new JsonArray([.. match.SecondActions.ToSaberScore()]),
                        ["clock"] = match.Clock.Timer.ToString("m\\:ss")
                    }
                ]));
            }
        }

        var node = new JsonObject();
        node["type"] = "pool";
        node["configuration"] = MatchGroup.Settings.ToSaberSportsSubmission(Squadron.Name, false);
        node["round"] = new JsonObject
        {
            ["index"] = index,
            ["participants"] = participantsNode,
            ["scores"] = scores,
            ["actions"] = actions
        };

        return node;
    }

    #endregion
}
