using Lightspeed.ViewModels;

namespace LightspeedNexus.Services;

/// <summary>
/// This is not a typical service in that it is not registered. It is used in DEBUG builds to generate scores for matches.
/// It is not used in release builds, and it is not intended to be used by the view models or any other part of the application.
/// </summary>
public static class ScoreGenerator
{
    private readonly static Random _random = new();

    public static (int First, int Second) GenerateScores(MatchViewModel match)
    {
        // the chance of the first participant winning
        double chance = 0.5;
        if (match.First.Participant.PowerLevel > match.Second.Participant.PowerLevel)
            chance += Math.Min(0.9, (match.First.Participant.PowerLevel - match.Second.Participant.PowerLevel) * 0.1);
        else if (match.First.Participant.PowerLevel < match.Second.Participant.PowerLevel)
            chance -= Math.Max(0.1, (match.Second.Participant.PowerLevel - match.First.Participant.PowerLevel) * 0.1);

        // we keep generating scores until one of the participants reaches the winning score or until we've generated
        // a certain number of scores (to simulate a match that goes to time)
        int first = 0; int second = 0;
        int maxMatches = match.Settings.WinningScore * 2 / 3;
        for (int i = 0; i < maxMatches && first < match.Settings.WinningScore && second < match.Settings.WinningScore; i++)
        {
            if (_random.NextDouble() < chance)
                first += GenerateScore();
            else
                second += GenerateScore();
        }

        if (first == second)
        {
            if (_random.NextDouble() < chance)
                first += GenerateScore();
            else
                second += GenerateScore();
        }

        return (first, second);
    }

    private static int GenerateScore() => _random.NextDouble() switch { >= 0.9 => 5, >= 0.4 => 3, _ => 1 };
}
