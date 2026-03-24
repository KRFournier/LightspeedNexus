using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Network;
using LightspeedNexus.Networking;
using LightspeedNexus.Services;
using System.ComponentModel;
using System.Diagnostics;

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
    [NotifyPropertyChangedFor(nameof(HasBye))]
    [NotifyPropertyChangedFor(nameof(HasFirst))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    public partial ScoreViewModel First { get; set; } = new();
    partial void OnFirstChanged(ScoreViewModel value) => CheckByeWinner();
    partial void OnFirstChanged(ScoreViewModel oldValue, ScoreViewModel newValue)
    {
        oldValue?.PropertyChanged -= ScorePropertyChanged;
        newValue?.PropertyChanged += ScorePropertyChanged;
    }
    public bool HasFirst => !First.Participant.IsEmpty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBye))]
    [NotifyPropertyChangedFor(nameof(HasSecond))]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    public partial ScoreViewModel Second { get; set; } = new();
    partial void OnSecondChanged(ScoreViewModel value) => CheckByeWinner();
    partial void OnSecondChanged(ScoreViewModel oldValue, ScoreViewModel newValue)
    {
        oldValue?.PropertyChanged -= ScorePropertyChanged;
        newValue?.PropertyChanged += ScorePropertyChanged;
    }
    public bool HasSecond => !Second.Participant.IsEmpty;

    [ObservableProperty]
    public partial bool IsMatchStarted { get; set; } = false;

    [ObservableProperty]
    public partial int? Number { get; set; }

    [ObservableProperty]
    public partial bool IsLive { get; set; } = false;

    public bool HasBye => First.Participant.IsBye || Second.Participant.IsBye;

    public bool IsEmpty => First.Participant is EmptyParticipantViewModel && Second.Participant is EmptyParticipantViewModel;

    #endregion

    #region Commands

    [RelayCommand]
    private async Task EditMatch()
    {
        try
        {
            var matchEdit = GetEditViewModel();
            if (matchEdit is not null)
            {
                var result = await EditDialog(matchEdit, "Set Final Match Score");
                if (result.IsOk)
                    UpdateMatch(result.Item);
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error editing a match: {e}");
        }
    }

    #endregion

    public MatchViewModel()
    {
        if (Design.IsDesignMode)
        {
            First = new ScoreViewModel(new PlayerViewModel("Player", "1")) { Points = 99, Seed = 1 };
            Second = new ScoreViewModel(new PlayerViewModel("Player", "With Long Name")) { Points = -99, Seed = 64 };
        }
        else
        {
            // Listen for the network's request for this match's state
            WeakReferenceMessenger.Default.Register<RequestMatchState, Guid>(this, Guid,
                (_, m) =>
                {
                    MatchState matchState = ToState();
                    matchState.Next = m.Next;
                    m.Reply(matchState);
                }
            );

            // List for live status updates
            WeakReferenceMessenger.Default.Register<SetLiveMessage, Guid>(this, Guid, (_, m) => IsLive = m.IsLive);
        }
    }

    public abstract Match? ToModel();

    public static MatchViewModel FromModel(Match? model) => model switch
    {
        StandardMatch standardMatch => StandardMatchViewModel.FromModel(standardMatch),
        null => new MatchNotFoundViewModel(),
        _ => throw new NotSupportedException($"Match type {model.GetType().Name} is not supported."),
    };

    public virtual MatchState ToState() => throw new NotImplementedException();

    public virtual MatchSummary ToSummary() => throw new NotImplementedException();

    public MatchEditViewModel? GetEditViewModel()
    {
        if (First is not null && Second is not null)
            return new()
            {
                First = [new() { Name = First.Participant.Name, Points = First.Points }],
                Second = [new() { Name = Second.Participant.Name, Points = Second.Points }]
            };

        return null;
    }

    public void UpdateMatch(MatchEditViewModel editedMatch)
    {
        if (editedMatch.First.Count == 0 || editedMatch.Second.Count == 0)
            throw new ArgumentException("Edited match must have at least one score for each participant.");

        First.Points = editedMatch.First[0].Points;
        Second.Points = editedMatch.Second[0].Points;
        IsMatchStarted = true;
        if (First.Points != Second.Points)
            Winner = First.Points > Second.Points ? First : Second;
        Save();
    }

    public void Save() => StorageService.WriteMatch(ToModel());

    protected void ScorePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Score.Participant))
        {
            OnPropertyChanged(nameof(HasBye));
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    /// <summary>
    /// Determines if the given participant is in in this match
    /// </summary>
    public bool Contains(ParticipantViewModel participant) => First.Participant == participant || Second.Participant == participant;

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
    public ScoreViewModel? Loser
    {
        get
        {
            if (Winner is not null)
            {
                if (Winner == First)
                    return Second;
                else if (Winner == Second)
                    return First;
            }
            return null;
        }
    }

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
    public int WinnerScore => IsFirstWinner ? First.Points : (IsSecondWinner ? Second.Points : 0);

    /// <summary>
    /// The loser's score
    /// </summary>
    public int LoserScore => IsFirstWinner ? Second.Points : (IsSecondWinner ? First.Points : 0);

    /// <summary>
    /// Determines if the match is completed based on whether or not there is a winner
    /// </summary>
    public virtual bool IsMatchCompleted => Winner is not null;

    /// <summary>
    /// Checks for a bye, and if there is one, sets the winner accordingly.
    /// If there is no bye, clears the winner to allow for normal match resolution
    /// </summary>
    public void CheckByeWinner()
    {
        if (First.Participant.IsBye && !Second.Participant.IsBye)
        {
            Winner = Second;
            return;
        }
        else if (!First.Participant.IsBye && Second.Participant.IsBye)
        {
            Winner = First;
            return;
        }

        Winner = null;
    }

    #endregion

    public override string ToString() => Number is not null ?
        $"#{Number}: {First} vs {Second}" :
        $"{First} vs {Second}";
}

/// <summary>
/// A placeholder for matches that could not be found
/// </summary>
public partial class MatchNotFoundViewModel : MatchViewModel
{
    public override Match? ToModel() => null;
    public override bool IsMatchCompleted => true;
    public override string ToString() => "Match Not Found!";
    public MatchNotFoundViewModel() { Guid = Guid.Empty; }
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
    public partial int Overtime { get; set; } = 0;

    public bool IsTimeUp => Timer <= TimeSpan.Zero;

    #endregion

    public Clock ToModel() => new()
    {
        Overtime = Overtime,
        Timer = Timer
    };

    public static ClockViewModel FromModel(Clock model) => new()
    {
        Overtime = model.Overtime,
        Timer = model.Timer
    };

    public override string ToString() => $"{Timer:mm\\:ss}" + (Overtime > 0 ? $" +{Overtime}OT" : string.Empty);

    public ClockState ToState() => new()
    {
        TimeRemaining = Timer,
        OvertimeCount = Overtime
    };

    public void FromState(ClockState? state)
    {
        if (state is null)
            return;

        Timer = state.TimeRemaining;
        Overtime = state.OvertimeCount;
    }
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

    public override string ToString()
    {
        if (InPriority)
            return PreviousPriority == Side.First ? $"<- {PriorityPoints}" : $"{PriorityPoints} ->";
        return "< 0 >";
    }

    public PriorityState ToState() => new()
    {
        PreviousPriority = PreviousPriority,
        PriorityPoints = PriorityPoints,
        InPriority = InPriority
    };

    public void FromState(PriorityState? state)
    {
        if (state is null)
            return;

        PreviousPriority = state.PreviousPriority;
        PriorityPoints = state.PriorityPoints;
        InPriority = state.InPriority;
    }
}