using LightspeedNetwork;
using System;

namespace LightspeedNexus.Models;

/// <summary>
/// A score for one side or the other. Score is abstract. The points could
/// be anything from life to action points, depending on the match.
/// </summary>
public sealed class Score
{
    /// <summary>
    /// A participant could be a player or a team.
    /// </summary>
    public Guid Participant { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The points for the player or team.
    /// </summary>
    public int Points { get; set; } = 0;

    /// <summary>
    /// A participant's seed in the bracket. If zero, then seed is not used
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// A match reference is used when the participant in this match is determined by the outcome of a previous match.
    /// </summary>
    public Guid? ParentMatchId { get; set; }

    /// <summary>
    /// The participant to use from the referenced match, either the winner or the loser
    /// </summary>
    public MatchOutcome? ParentMatchReference { get; set; }

    /// <summary>
    /// The number of minor violations this match
    /// </summary>
    public int MinorViolations { get; set; } = 0;
}