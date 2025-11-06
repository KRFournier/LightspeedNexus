using LightspeedNexus.ViewModels;
using System;
using System.Collections.Generic;

namespace LightspeedNexus.Models;

/// <summary>
/// A player is a contestant in a particular match. Each player has a score that may or may not
/// contribute to the overall score of their team, which depends on the game format.
/// </summary>
public sealed class Player
{
    public Guid Fighter { get; set; } = Guid.NewGuid();
    public int MinorViolations { get; set; } = 0;
    public int Score { get; set; } = 0;

    public Player() { }
    public Player(Guid player, int score, int minorViolations)
    {
        Fighter = player;
        Score = score;
        MinorViolations = minorViolations;
    }

    public ParticipantViewModel ToViewModel(IReadOnlyList<ContestantViewModel> players) => new(this, players);
}

/// <summary>
/// Specifies the priority state for a match
/// </summary>
/// <remarks>Use this enumeration to indicate which player currently has priority, or to represent the absence of
/// priority.</remarks>
public enum PriorityState
{
    None = 0,
    PlayerOne,
    PlayerTwo,
}

/// <summary>
/// A match between two players, including their scores and actions.
/// </summary>
public class Match : CollectionObject
{
    public int Number { get; set; } = 0;
    public MatchSettings Settings { get; set; } = new();
    public TimeSpan TimeRemaining { get; set; } = TimeSpan.FromSeconds(90);
    public int OvertimeCount { get; set; } = 0;
    public int[] Players { get; set; } = [];
    public Team? PlayerOne { get; set; }
    public Team? PlayerTwo { get; set; }
    public bool IsMatchStarted { get; set; } = false;
    public Action[] Actions { get; set; } = [];
    public PriorityState PreviousPriority { get; set; } = PriorityState.None;
    public int PriorityPoints { get; set; } = 3;
    public bool InPriority { get; set; } = false;

    public Match() { TimeRemaining = Settings.TimeLimit; }

    public Match(Guid id, int number, MatchSettings settings, TimeSpan timeRemaining, int overtimeCount,
        int[] players, Team? first, Team? second, bool isMatchStarted, Action[] actions, PriorityState previousPriority,
        int priorityPoints, bool inPriority) : base(id)
    {
        Number = number;
        Settings = settings;
        TimeRemaining = timeRemaining;
        OvertimeCount = overtimeCount;
        Players = players;
        PlayerOne = first;
        PlayerTwo = second;
        IsMatchStarted = isMatchStarted;
        Actions = actions;
        PreviousPriority = previousPriority;
        PriorityPoints = priorityPoints;
        InPriority = inPriority;
    }

    public MatchViewModel ToViewModel(IReadOnlyList<ContestantViewModel> fullRoster) => new(this, fullRoster);
}
