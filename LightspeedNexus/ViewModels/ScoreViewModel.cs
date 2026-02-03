using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNetwork;
using LightspeedNexus.Models;
using System;
using System.ComponentModel;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A participant(s)' score in a match
/// </summary>
public partial class ScoreViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial ParticipantViewModel Participant { get; set; }

    [ObservableProperty]
    public partial int Points { get; set; } = 0;

    [ObservableProperty]
    public partial int? Seed { get; set; }

    [ObservableProperty]
    public partial int MinorViolations { get; set; } = 0;

    /// <summary>
    /// Determines if this player/team is out of the match, either due to disqualification or
    /// because this score represents a bye
    /// </summary>
    public bool IsOut => Participant.IsDisqualified || Participant.IsBye;

    private readonly MatchViewModel? _parentMatch;
    private readonly MatchOutcome? _parentMatchRef;

    #endregion

    public ScoreViewModel()
    {
        Participant = ParticipantViewModel.Empty;
    }

    public ScoreViewModel(ParticipantViewModel participant)
    {
        Participant = participant;

        if (!participant.IsBye)
        {
            WeakReferenceMessenger.Default.Register<ParticipantDisqualifedChanged, Guid>(
                this,
                participant.Guid,
                (r, m) => OnPropertyChanged(nameof(IsOut))
            );
        }
    }

    public ScoreViewModel(ScoreViewModel score) : this(score.Participant)
    {
        Seed = score.Seed;
    }

    public Score ToModel() => new()
    {
        Participant = Participant.Guid,
        Points = Points,
        Seed = Seed,
        ParentMatchId = _parentMatch?.Guid,
        ParentMatchReference = _parentMatchRef,
        MinorViolations = MinorViolations
    };

    public static ScoreViewModel FromModel(Score model)
    {
        if (model.ParentMatchId is not null && model.ParentMatchReference is not null)
        {
            var parentMatch = StrongReferenceMessenger.Default.Send(new RequestBracketMatch(model.ParentMatchId.Value));
            if (parentMatch is not null)
            {
                var vm = model.ParentMatchReference.Value switch
                {
                    MatchOutcome.Winner => WinnerOf(parentMatch),
                    MatchOutcome.Loser => LoserOf(parentMatch),
                    _ => throw new NotSupportedException($"Match outcome {model.ParentMatchReference.Value} is not supported.")
                };
                vm.Points = model.Points;
                vm.Seed = model.Seed;
                vm.MinorViolations = model.MinorViolations;
                return vm;
            }
        }

        return new(StrongReferenceMessenger.Default.Send(new RequestParticipant(model.Participant)))
        {
            Points = model.Points,
            Seed = model.Seed
        };
    }

    public override string ToString() => _parentMatch is not null && Participant.IsEmpty ? $"{_parentMatchRef} of ({_parentMatch})" : Participant.Name;

    #region Match Outcome Factories

    protected ScoreViewModel(MatchViewModel parentMatch, MatchOutcome outcome) : this()
    {
        _parentMatch = parentMatch;
        _parentMatchRef = outcome;
        _parentMatch.PropertyChanged += PropertyChangedEventHandler;

        if (outcome == MatchOutcome.Winner && parentMatch.Winner is not null)
        {
            Participant = parentMatch.Winner.Participant;
            Seed = parentMatch.Winner.Seed;
        }
        else if (outcome == MatchOutcome.Loser && parentMatch.Loser is not null)
        {
            Participant = parentMatch.Loser.Participant;
            Seed = parentMatch.Loser.Seed;
        }
    }

    public static ScoreViewModel WinnerOf(MatchViewModel match) => new(match, MatchOutcome.Winner);

    public static ScoreViewModel LoserOf(MatchViewModel match) => new(match, MatchOutcome.Loser);

    private void PropertyChangedEventHandler(object? sender, PropertyChangedEventArgs e)
    {
        if (_parentMatchRef == MatchOutcome.Winner && e.PropertyName == nameof(MatchViewModel.Winner))
        {
            Participant = _parentMatch?.Winner?.Participant ?? ParticipantViewModel.Empty;
            Seed = _parentMatch?.Winner?.Seed;
        }
        else if (_parentMatchRef == MatchOutcome.Loser && e.PropertyName == nameof(MatchViewModel.Loser))
        {
            Participant = _parentMatch?.Loser?.Participant ?? ParticipantViewModel.Empty;
            Seed = _parentMatch?.Loser?.Seed;
        }
    }

    #endregion

    public ScoreState ToState() => new()
    {
        Participant = Participant.ToState(),
        Points = Points,
        Seed = Seed,
        MinorViolationCount = MinorViolations
    };

    public void FromState(ScoreState? state)
    {
        if (state is null)
            return;

        Participant.FromState(state.Participant);
        Points = state.Points;
        Seed = state.Seed;
        MinorViolations = state.MinorViolationCount;
    }
}