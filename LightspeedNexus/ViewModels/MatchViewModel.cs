using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// The base class for all matches
/// </summary>
public abstract partial class MatchViewModel : ViewModelBase
{
    #region Properties

    public Guid Guid { get; protected set; } = Guid.NewGuid();

    [ObservableProperty]
    public partial MatchSettingsViewModel Settings { get; set; } = new();

    [ObservableProperty]
    public partial bool IsMatchStarted { get; set; } = false;

    public abstract bool IsMatchCompleted { get; }

    [ObservableProperty]
    public partial int? Number { get; set; }

    [ObservableProperty]
    public partial bool IsLive { get; set; } = false;

    #endregion

    public abstract Match? ToModel();

    public static MatchViewModel FromModel(Match? model) => model switch
    {
        StandardMatch standardMatch => StandardMatchViewModel.FromModel(standardMatch),
        null => new MatchNotFoundViewModel(),
        _ => throw new NotSupportedException($"Match type {model.GetType().Name} is not supported."),
    };

    public abstract MatchEditViewModel GetEditViewModel();

    public virtual void UpdateMatch(MatchEditViewModel editedMatch) { }

    public void Save() => StorageService.WriteMatch(ToModel());
}

/// <summary>
/// A placeholder for matches that could not be found
/// </summary>
public partial class MatchNotFoundViewModel : MatchViewModel
{
    public override Match? ToModel() => null;
    public override bool IsMatchCompleted => true;
    public override MatchEditViewModel GetEditViewModel() => new();
    public override void UpdateMatch(MatchEditViewModel editedMatch) { }
}

/// <summary>
/// A clock is a set of one (or more) timers with support for overtime.
/// The match ends if any timer reaches zero.
/// </summary>
public partial class ClockViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial TimeSpan Timer { get; set; } = TimeSpan.FromSeconds(90);

    [ObservableProperty]
    public partial TimeSpan? SecondaryTimer { get; set; }

    [ObservableProperty]
    public partial int Overtime { get; set; } = 0;

    public bool IsTimeUp => Timer <= TimeSpan.Zero || SecondaryTimer <= TimeSpan.Zero;

    #endregion

    public Clock ToModel() => new()
    {
        Overtime = Overtime,
        Timer = Timer,
        SecondaryTimer = SecondaryTimer
    };

    public static ClockViewModel FromModel(Clock model) => new()
    {
        Overtime = model.Overtime,
        Timer = model.Timer,
        SecondaryTimer = model.SecondaryTimer
    };
}

/// <summary>
/// Adds support for priority to a match
/// </summary>
public partial class PriorityViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial Side PreviousPriority { get; set; } = Side.Neither;

    [ObservableProperty]
    public partial int PriorityPoints { get; set; } = 3;

    [ObservableProperty]
    public partial bool InPriority { get; set; } = false;

    #endregion

    public Priority ToModel() => new()
    {
        PreviousPriority = PreviousPriority,
        PriorityPoints = PriorityPoints,
        InPriority = InPriority
    };

    public static PriorityViewModel FromModel(Priority model) => new()
    {
        PreviousPriority = model.PreviousPriority,
        PriorityPoints = model.PriorityPoints,
        InPriority = model.InPriority
    };
}

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

    /// <summary>
    /// Determines if this player/team is out of the match, either due to disqualification or
    /// because this score represents a bye
    /// </summary>
    public bool IsOut => Participant.IsDisqualified;

    #endregion

    public Score ToModel() => new()
    {
        Participant = Participant.Guid,
        Points = Points
    };

    public static ScoreViewModel FromModel(Score model) => new()
    {
        Participant = StrongReferenceMessenger.Default.Send(new RequestParticipant(model.Participant)),
        Points = model.Points
    };
}