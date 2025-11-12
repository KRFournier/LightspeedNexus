using System;
using System.Collections.Generic;
using System.Linq;

namespace LightspeedNexus.Models;

public sealed record Grading(
    string Rating,
    Range FencersNeeded,
    (int As, int Bs, int Cs, int Ds, int Es) RegistrationCriteria,
    int TopX,
    (int As, int Bs, int Cs, int Ds, int Es) QualifyingCriteria,
    Rank[] Awards
    );

public static class GradingsChart
{
    public static readonly Grading[] Chart = [
        new("A1", 5..6,             (1, 0, 1, 0, 0), 2,  (1, 0, 0, 0, 1), ["A", "B"]),
        new("A2", 7..12,            (1, 0, 1, 1, 2), 4,  (1, 0, 1, 0, 1), ["A", "B", "C", "C"]),
        new("A2", 7..12,            (0, 1, 1, 2, 1), 4,  (0, 1, 1, 1, 0), ["A", "B", "C", "C"]),
        new("A3", 13..24,           (1, 1, 2, 3, 3), 8,  (1, 1, 1, 2, 1), ["A", "B", "C", "C", "D", "D", "D", "D"]),
        new("A3", 13..24,           (0, 2, 2, 3, 4), 8,  (0, 2, 2, 1, 1), ["A", "B", "C", "C", "D", "D", "D", "D"]),
        new("A4", 25..int.MaxValue, (3, 3, 3, 4, 4), 16, (2, 3, 2, 2, 2), ["A", "B", "C", "C", "D", "D", "D", "D", "E", "E", "E", "E", "E", "E", "E", "E"]),
        new("A4", 25..int.MaxValue, (0, 4, 5, 5, 6), 16, (0, 4, 3, 3, 3), ["A", "B", "C", "C", "D", "D", "D", "D", "E", "E", "E", "E", "E", "E", "E", "E"]),

        new("B1", 5..6,             (0, 1, 0, 1, 0), 2,  (0, 1, 0, 0, 0), ["B", "C"]),
        new("B2", 7..12,            (0, 1, 0, 2, 1), 4,  (0, 1, 0, 1, 1), ["B", "C", "D", "D"]),
        new("B2", 7..12,            (0, 0, 1, 2, 2), 4,  (0, 0, 1, 1, 0), ["B", "C", "D", "D"]),
        new("B3", 13..24,           (0, 1, 2, 2, 3), 8,  (0, 1, 1, 2, 2), ["B", "C", "D", "D", "E", "E", "E", "E"]),
        new("B3", 13..24,           (0, 0, 2, 3, 4), 8,  (0, 0, 2, 2, 2), ["B", "C", "D", "D", "E", "E", "E", "E"]),
        new("B4", 25..int.MaxValue, (0, 3, 3, 4, 3), 16, (0, 3, 2, 2, 1), ["B", "B", "C", "C", "D", "D", "D", "D", "E", "E", "E", "E", "E", "E", "E", "E"]),
        new("B4", 25..int.MaxValue, (0, 0, 5, 5, 6), 16, (0, 0, 4, 4, 3), ["B", "B", "C", "C", "D", "D", "D", "D", "E", "E", "E", "E", "E", "E", "E", "E"]),

        new("C1", 5..6,             (0, 0, 1, 1, 0), 2,  (0, 0, 1, 0, 0), ["C", "D"]),
        new("C2", 7..12,            (0, 0, 1, 0, 3), 4,  (0, 0, 1, 0, 1), ["C", "D", "E", "E"]),
        new("C2", 7..12,            (0, 0, 0, 2, 2), 4,  (0, 0, 0, 2, 0), ["C", "D", "E", "E"]),
        new("C3", 13..24,           (0, 0, 1, 3, 2), 8,  (0, 0, 2, 1, 0), ["C", "D", "D", "D", "E", "E", "E", "E"]),
        new("C3", 13..24,           (0, 0, 0, 3, 5), 8,  (0, 0, 0, 3, 2), ["C", "D", "D", "D", "E", "E", "E", "E"]),
        new("C4", 25..int.MaxValue, (0, 0, 3, 4, 4), 16, (0, 0, 3, 2, 3), ["C", "C", "D", "D", "D", "D", "D", "D", "E", "E", "E", "E", "E", "E", "E", "E"]),
        new("C4", 25..int.MaxValue, (0, 0, 0, 7, 7), 16, (0, 0, 0, 5, 6), ["C", "C", "D", "D", "D", "D", "D", "D", "E", "E", "E", "E", "E", "E", "E", "E"]),

        new("D1", 5..6,             (0, 0, 0, 1, 0), 2,  (0, 0, 0, 1, 0), ["D", "E"]),
        new("D2", 7..12,            (0, 0, 0, 1, 0), 4,  (0, 0, 0, 1, 0), ["D", "E", "E", "E"]),
        new("D2", 7..12,            (0, 0, 0, 0, 2), 4,  (0, 0, 0, 0, 2), ["D", "E", "E", "E"]),
        new("D3", 13..24,           (0, 0, 0, 1, 1), 8,  (0, 0, 0, 1, 1), ["D", "D", "D", "E", "E", "E", "E", "E"]),
        new("D3", 13..24,           (0, 0, 0, 0, 3), 8,  (0, 0, 0, 0, 3), ["D", "D", "D", "E", "E", "E", "E", "E"]),
        new("D4", 25..int.MaxValue, (0, 0, 0, 2, 2), 16, (0, 0, 0, 2, 1), ["D", "D", "D", "D", "D", "D", "D", "D", "E", "E", "E", "E", "E", "E", "E", "E"]),
        new("D4", 25..int.MaxValue, (0, 0, 0, 0, 6), 16, (0, 0, 0, 0, 5), ["D", "D", "D", "D", "D", "D", "D", "D", "E", "E", "E", "E", "E", "E", "E", "E"]),

        new("E1", 5..6,             (0, 0, 0, 0, 0), 2,  (0, 0, 0, 0, 0), ["E", "E"]),
        new("E2", 7..12,            (0, 0, 0, 0, 0), 4,  (0, 0, 0, 0, 0), ["E", "E", "E", "E"]),
        new("E3", 13..24,           (0, 0, 0, 0, 0), 8,  (0, 0, 0, 0, 0), ["E", "E", "E", "E", "E", "E", "E", "E"]),
        new("E4", 25..int.MaxValue, (0, 0, 0, 0, 0), 16, (0, 0, 0, 0, 0), ["E", "E", "E", "E", "E", "E", "E", "E", "E", "E", "E", "E", "E", "E", "E", "E"]),
        ];

    /// <summary>
    /// Counts the number of ranks of each letter in the given collection.
    /// </summary>
    private static (int count, (int As, int Bs, int Cs, int Ds, int Es)) CountRanks(IEnumerable<Rank> playerRanks)
    {
        int count = 0, As = 0, Bs = 0, Cs = 0, Ds = 0, Es = 0;
        foreach (var rank in playerRanks)
        {
            switch (rank.Letter)
            {
                case 'A': count++; As++; break;
                case 'B': count++; Bs++; break;
                case 'C': count++; Cs++; break;
                case 'D': count++; Ds++; break;
                case 'E': count++; Es++; break;
                default: count++; break;
            }
        }
        return (count, (As, Bs, Cs, Ds, Es));
    }

    /// <summary>
    /// Helper method that determines if the given rank counts meet counts needed. This comparison
    /// is a little more complex than a simple greater-than-or-equal-to, because higher ranks can
    /// fulfill lower rank requirements.
    /// 
    /// The algorithm works by iterating through each rank needed, from highest to lowest, and
    /// attempting to fulfill that need using the highest available ranks first.
    /// </summary>
    private static bool MeetsCriteria(
        (int As, int Bs, int Cs, int Ds, int Es) rankCount,
        (int As, int Bs, int Cs, int Ds, int Es) ranksNeeded)
    {
        while (ranksNeeded.As > 0)
        {
            if (rankCount.As > 0)
                rankCount.As--;
            else
                return false;
            ranksNeeded.As--;
        }

        while (ranksNeeded.Bs > 0)
        {
            if (rankCount.As > 0)
                rankCount.As--;
            else if (rankCount.Bs > 0)
                rankCount.Bs--;
            else
                return false;
            ranksNeeded.Bs--;
        }

        while (ranksNeeded.Cs > 0)
        {
            if (rankCount.As > 0)
                rankCount.As--;
            else if (rankCount.Bs > 0)
                rankCount.Bs--;
            else if (rankCount.Cs > 0)
                rankCount.Cs--;
            else
                return false;
            ranksNeeded.Cs--;
        }

        while (ranksNeeded.Ds > 0)
        {
            if (rankCount.As > 0)
                rankCount.As--;
            else if (rankCount.Bs > 0)
                rankCount.Bs--;
            else if (rankCount.Cs > 0)
                rankCount.Cs--;
            else if (rankCount.Ds > 0)
                rankCount.Ds--;
            else
                return false;
            ranksNeeded.Ds--;
        }

        while (ranksNeeded.Es > 0)
        {
            if (rankCount.As > 0)
                rankCount.As--;
            else if (rankCount.Bs > 0)
                rankCount.Bs--;
            else if (rankCount.Cs > 0)
                rankCount.Cs--;
            else if (rankCount.Ds > 0)
                rankCount.Ds--;
            else if (rankCount.Es > 0)
                rankCount.Es--;
            else
                return false;
            ranksNeeded.Es--;
        }

        return true;
    }

    /// <summary>
    /// Finds the best possible grading for the tournament, based on the given registrees' ranks.
    /// </summary>
    public static Grading? FindInitial(IEnumerable<Rank> registeredRanks)
    {
        var (registeredCount, rankCounts) = CountRanks(registeredRanks);

        return Chart.Where(g => registeredCount >= g.FencersNeeded.Start.Value && registeredCount <= g.FencersNeeded.End.Value)
            .FirstOrDefault(g => MeetsCriteria(rankCounts, g.RegistrationCriteria));
    }

    /// <summary>
    /// Finds the final grading for the tournament, based on the given registrees' ranks and the top X ranks in the brackets
    /// </summary>
    public static Grading? FindFinal(IEnumerable<Rank> registeredRanks, IEnumerable<Rank> topRanks)
    {
        var (registeredCount, registeredRankCounts) = CountRanks(registeredRanks);
        var (topCount, topRankCounts) = CountRanks(topRanks);

        return Chart.Where(g => registeredCount >= g.FencersNeeded.Start.Value && registeredCount <= g.FencersNeeded.End.Value && topCount == g.TopX)
            .FirstOrDefault(g => MeetsCriteria(registeredRankCounts, g.RegistrationCriteria) && MeetsCriteria(topRankCounts, g.QualifyingCriteria));
    }

    /// <summary>
    /// Returns the Top X players used for ranking in the tournament grading based on the number of players in the tournament.
    /// </summary>
    public static int GetTopX(int playerCount) =>
        Chart.FirstOrDefault(g => playerCount >= g.FencersNeeded.Start.Value && playerCount <= g.FencersNeeded.End.Value)?.TopX ?? 0;

    /// <summary>
    /// Determines if a tournament with the given number of players is rankable.
    /// </summary>
    public static bool IsRankable(int playerCount) =>
        Chart.Any(g => playerCount >= g.FencersNeeded.Start.Value && playerCount <= g.FencersNeeded.End.Value);
}
