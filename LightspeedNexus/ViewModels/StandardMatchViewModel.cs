using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A match
/// </summary>
public partial class StandardMatchViewModel : MatchViewModel
{
    #region Properties

    [ObservableProperty]
    public partial ScoreViewModel First { get; set; }

    [ObservableProperty]
    public partial ScoreViewModel Second { get; set; }

    [ObservableProperty]
    public partial ClockViewModel Clock { get; set; } = new();

    [ObservableProperty]
    public partial Models.Action[] Actions { get; set; } = [];

    [ObservableProperty]
    public partial PriorityViewModel Priority { get; set; } = new();

    #endregion

    public override StandardMatch ToModel() => new()
    {
        Id = Guid,
        Clock = Clock.ToModel(),
        First = First.ToModel(),
        Second = Second.ToModel(),
        IsMatchStarted = IsMatchStarted,
        Actions = Actions,
        Priority = Priority.ToModel(),
        Winner = WinningSide
    };

    public static StandardMatchViewModel FromModel(StandardMatch model)
    {
        StandardMatchViewModel vm = new()
        {
            Guid = model.Id,
            Clock = ClockViewModel.FromModel(model.Clock),
            First = ScoreViewModel.FromModel(model.First),
            Second = ScoreViewModel.FromModel(model.Second),
            IsMatchStarted = model.IsMatchStarted,
            Actions = model.Actions,
            Priority = PriorityViewModel.FromModel(model.Priority),
            WinningSide = model.Winner
        };
        return vm;
    }

    public override MatchEditViewModel GetEditViewModel() => new()
    {
        First = [new() { Name = First.Participant.Name, Points = First.Points }],
        Second = [new() { Name = Second.Participant.Name, Points = Second.Points }]
    };

    public override void UpdateMatch(MatchEditViewModel editedMatch)
    {
        if(editedMatch.First.Count == 0 || editedMatch.Second.Count == 0)
            throw new ArgumentException("Edited match must have at least one score for each participant.");

        First.Points = editedMatch.First[0].Points;
        Second.Points = editedMatch.Second[0].Points;
        IsMatchStarted = true;
        if (First.Points != Second.Points)
            Winner = First.Points > Second.Points ? First : Second;
        Save();
    }

    #region Winner

    /// <summary>
    /// The winner of the match, if there is one
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Loser))]
    [NotifyPropertyChangedFor(nameof(IsMatchCompleted))]
    [NotifyPropertyChangedFor(nameof(WinnerScore))]
    [NotifyPropertyChangedFor(nameof(LoserScore))]
    [NotifyPropertyChangedFor(nameof(IsFirstWinner))]
    [NotifyPropertyChangedFor(nameof(IsSecondWinner))]
    [NotifyPropertyChangedFor(nameof(WinningSide))]
    public partial ScoreViewModel? Winner { get; set; } = null;

    /// <summary>
    /// The winner referenced by position in the match
    /// </summary>
    public Side WinningSide
    {
        get => Winner switch
        {
            _ when Winner == First => Side.First,
            _ when Winner == Second => Side.Second,
            _ => Side.Neither
        };

        set => Winner = value switch
        {
            Side.First => First,
            Side.Second => Second,
            _ => null
        };
    }

    /// <summary>
    /// The loser of the match
    /// </summary>
    public ScoreViewModel? Loser => Winner switch
    {
        _ when Winner == First => Second,
        _ when Winner == Second => First,
        _ => null
    };

    /// <summary>
    /// Determines if the winner is first
    /// </summary>
    public bool IsFirstWinner => Winner == First;

    /// <summary>
    /// Determines if the winner is red
    /// </summary>
    public bool IsSecondWinner => Winner == Second;

    /// <summary>
    /// The winner's score
    /// </summary>
    public int WinnerScore => IsFirstWinner ? First!.Points : (IsSecondWinner ? Second!.Points : 0);

    /// <summary>
    /// The loser's score
    /// </summary>
    public int LoserScore => IsFirstWinner ? Second!.Points : (IsSecondWinner ? First!.Points : 0);

    /// <summary>
    /// Determines if the match is completed based on whether or not there is a winner
    /// </summary>
    public override bool IsMatchCompleted => Winner is not null;

    /// <summary>
    /// Updates the Winner
    /// </summary>
    private void CheckWinners()
    {
        if (First is null || Second is null)
            return;

        // check for autowins
        if (Second.IsOut && !First.IsOut)
        {
            Winner = First;
        }
        else if (First.IsOut && !Second.IsOut)
        {
            Winner = Second;
        }
        else
        {
            if (First.Points == Second.Points)
                Winner = null;
            else
            {
                // check for winner at end of time
                if (Clock.IsTimeUp)
                {
                    if (First.Points > Second.Points)
                        Winner = First;
                    else if (Second.Points > First.Points)
                        Winner = Second;
                }

                // check for winning score
                else
                {
                    if (First.Points >= Settings.WinningScore)
                        Winner = First;
                    else if (Second.Points >= Settings.WinningScore)
                        Winner = Second;
                }
            }
        }
    }

    #endregion
}
