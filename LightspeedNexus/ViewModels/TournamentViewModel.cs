using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Lightspeed.Network;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.Networking;
using LightspeedNexus.Services;
using System.Text.Json.Nodes;

namespace LightspeedNexus.ViewModels;

#region Messages

public sealed class RequestIsRanked : RequestMessage<bool?> { }
public sealed class RequestFinalGrading : RequestMessage<Grading?> { }
public sealed class RequestTournamentValue : RequestMessage<int> { }
public sealed class SaveAndCloseMessage { }
public sealed class RequestSubmittable : RequestMessage<bool> { }
public sealed class RequestSaberScoreJson(string signature) : RequestMessage<string>
{
    public readonly string Signature = signature;
}

#endregion

public partial class TournamentViewModel : ViewModelBase, IDisposable,
    IRecipient<NextStageMessage>, IRecipient<PreviousStageMessage>,
    IRecipient<RosterChangedMessage>, IRecipient<BracketRoundCompleted>,
    IRecipient<RequestIsRanked>, IRecipient<RequestFinalGrading>,
    IRecipient<RequestTournamentValue>, IRecipient<SaveAndCloseMessage>,
    IRecipient<RequestSubmittable>, IRecipient<RequestSaberScoreJson>
{
    #region Properties

    /// <summary>
    /// The tournament's unique identifier
    /// </summary>
    public Guid Guid { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// The initial stage of the tournament
    /// </summary>
    public SetupStageViewModel SetupStage { get; set; }

    /// <summary>
    /// The previously completed stages of the tournament
    /// </summary>
    public IEnumerable<StageViewModel> PreviousStages
    {
        get
        {
            StageViewModel stage = SetupStage;
            while (stage.Next is not null)
            {
                yield return stage;
                stage = stage.Next;
            }
        }
    }

    /// <summary>
    /// The current stage of the tournament
    /// </summary>
    public StageViewModel CurrentStage
    {
        get
        {
            StageViewModel stage = SetupStage;
            while (stage.Next is not null)
                stage = stage.Next;
            return stage;
        }
    }

    /// <summary>
    /// The previous stage of the tournament
    /// </summary>
    public StageViewModel? PreviousStage => CurrentStage.Previous;

    /// <summary>
    /// Finds the stage of the given type in the tournament, or null if it doesn't exist
    /// </summary>
    public T? FindStage<T>() where T : StageViewModel
    {
        StageViewModel? stage = SetupStage;
        while (stage is not null)
        {
            if (stage is T t)
                return t;
            stage = stage.Next;
        }
        return null;
    }

    #endregion

    #region Commands

    /// <summary>
    /// Saves the tournamnet and returns to the main menu
    /// </summary>
    [RelayCommand]
    private void GoHome()
    {
        Save();
        WeakReferenceMessenger.Default.Send<NavigateHomeMessage>();
    }

    #endregion

    #region Message Handlers

    public void Receive(NextStageMessage message)
    {
        OnPropertyChanged(nameof(PreviousStages));
        OnPropertyChanged(nameof(CurrentStage));
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(InitialRank));
        OnPropertyChanged(nameof(FinalRank));
        Save();
    }

    public void Receive(PreviousStageMessage message)
    {
        message.CurrentStage?.Dispose();
        message.PreviousStage?.Next = null;
        OnPropertyChanged(nameof(PreviousStages));
        OnPropertyChanged(nameof(CurrentStage));
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(InitialRank));
        OnPropertyChanged(nameof(FinalRank));
        Save();
    }

    public void Receive(RosterChangedMessage message)
    {
        OnPropertyChanged(nameof(IsRanked));
        OnPropertyChanged(nameof(InitialRank));
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(FinalRank));
    }

    public void Receive(BracketRoundCompleted message) => OnPropertyChanged(nameof(FinalRank));

    public void Receive(RequestIsRanked message) => message.Reply(IsRanked);

    public void Receive(RequestFinalGrading message) => message.Reply(GetFinalGrading());

    public void Receive(RequestTournamentValue message) => message.Reply(Value);

    public void Receive(SaveAndCloseMessage message) => GoHome();

    public void Receive(RequestSubmittable message)
    {
        bool canSubmit = SetupStage is not null && SetupStage.GameMode == GameMode.Standard;
        message.Reply(canSubmit);
    }

    public void Receive(RequestSaberScoreJson message)
    {
        var node = ToSaberSportsSubmission();
        node["signature"] = message.Signature;
        message.Reply(node.ToJsonString());
    }

    #endregion

    private readonly bool _loading = true;

    /// <summary>
    /// Creates a brand new tournament
    /// </summary>
    public TournamentViewModel()
    {
        SetupStage = new();
        StrongReferenceMessenger.Default.RegisterAll(this);
        _loading = false;

        SetupNetworkListeners();
    }

    /// <summary>
    /// Loads an existing tournament
    /// </summary>
    public TournamentViewModel(Tournament model)
    {
        Guid = model.Id;
        StrongReferenceMessenger.Default.RegisterAll(this);
        SetupStage = SetupStageViewModel.FromModel(model.SetupStage);

        // have to set this manually since the stages are loaded in bulk and won't trigger the property changed events
        FindStage<BracketStageViewModel>()?.IsRanked = IsRanked;
        _loading = false;

        SetupNetworkListeners();
    }

    /// <summary>
    /// Converts to a <see cref="Tournament"/>
    /// </summary>
    public Tournament ToModel() => new()
    {
        Id = Guid,
        SetupStage = SetupStage.ToModel(),
        IsCompleted = CurrentStage.IsTournamentCompleted
    };

    /// <summary>
    /// Saves the tournament
    /// </summary>
    public void Save()
    {
        if (!_loading && SetupStage.CanBegin())
        {
            StorageService.WriteTournament(ToModel());
            SetupStage.OnTournamentSaved();
        }
    }

    /// <summary>
    /// Cleans up messenger registrations
    /// </summary>
    public void Dispose()
    {
        StrongReferenceMessenger.Default.UnregisterAll(this);
        SetupStage.Dispose();
        GC.SuppressFinalize(this);
    }

    #region Value and Rank

    /// <summary>
    /// Determines if the tournament is ranked
    /// </summary>
    public bool IsRanked => SetupStage.GameMode == GameMode.Standard && GradingsChart.IsRankable(SetupStage.Registrees.Count);

    [ObservableProperty]
    public partial string? TopRanked { get; protected set; }

    /// <summary>
    /// The tournament's initial rank
    /// </summary>
    public string? InitialRank
    {
        get
        {
            if (!IsRanked)
                return "Unranked";

            var grading = GradingsChart.FindInitial(SetupStage.Registrees.Select(r => r.Rank));
            if (grading is not null)
            {
                TopRanked = $"Top {grading.TopX}";
                return grading.Rating;
            }

            return null;
        }
    }

    /// <summary>
    /// The tournament's initial rank
    /// </summary>
    public string? FinalRank
    {
        get
        {
            if (!IsRanked)
                return "Unranked";
            return GetFinalGrading()?.Rating;
        }
    }

    /// <summary>
    /// Calculates the value of the tournament based on the registrees' ranks
    /// </summary>
    public int Value => SetupStage.Registrees.Select(r => r.Rank.Power).Sum();

    /// <summary>
    /// Finds the grading that corresponds to the tournament's final rank, or null if it can't be determined
    /// </summary>
    /// <returns></returns>
    protected Grading? GetFinalGrading()
    {
        int topX = GradingsChart.GetTopX(SetupStage.Registrees.Count);
        if (topX > 0)
        {
            var topPlayers = FindStage<BracketStageViewModel>()?.GetTopXParticipants(topX);
            if (topPlayers is not null)
            {
                var grading = GradingsChart.FindFinal(
                    SetupStage.Registrees.Select(r => r.Rank),
                    topPlayers.OfType<PlayerViewModel>().Select(p => p.Rank)
                    );

                if (grading is not null)
                    return grading;
            }
        }

        return null;
    }

    #endregion

    #region Saber Sports

    /// <summary>
    /// Creates the json for submitting the tournament to saber-sports
    /// </summary>
    public JsonNode ToSaberSportsSubmission()
    {
        PoolsStageViewModel pools = FindStage<PoolsStageViewModel>() ??
            throw new InvalidOperationException("Cannot create Saber Sports submission for tournament without pools stage.");
        BracketStageViewModel bracket = FindStage<BracketStageViewModel>() ??
            throw new InvalidOperationException("Cannot create Saber Sports submission for tournament without bracket stage.");
        ResultsStageViewModel results = FindStage<ResultsStageViewModel>() ??
            throw new InvalidOperationException("Cannot create Saber Sports submission for tournament without results stage.");

        var rounds = new JsonArray();
        for (int i = 0; i < pools.Pools.Count; i++)
            rounds.Add(pools.Pools[i].ToSaberSportsSubmission(i));
        rounds.Add(bracket.ToSaberSportsSubmission());

        var node = new JsonObject
        {
            ["uuid"] = $"lsn{Guid}",
            ["title"] = SetupStage.EventName is not null ? $"{SetupStage.EventName} {SetupStage.Title}" : SetupStage.Title,
            ["date"] = SetupStage.Date?.ToString("yyyy-MM-dd"),
            ["gender"] = SetupStage.Demographic switch { Demographic.Women => "women", Demographic.Cadet => "cadet", _ => "mixed" },
            ["level"] = SetupStage.ReyAllowed && !SetupStage.RenAllowed && !SetupStage.TanoAllowed ? "rey" :
                        !SetupStage.ReyAllowed && SetupStage.RenAllowed && !SetupStage.TanoAllowed ? "ren" : "mixed",
            ["completed"] = CurrentStage.IsTournamentCompleted,
            ["participants"] = new JsonArray([.. results.Placements.Select(p => p.ToSaberSportsSubmission())]),
            ["rounds"] = rounds
        };
        return node;
    }

    #endregion

    #region Networking

    public static int Connections => NetworkService.Connections;

    public static string? Address => NetworkService.GetIPAddress();

    /// <summary>
    /// Sets up handlers for messages that come from the network service
    /// </summary>
    private void SetupNetworkListeners()
    {
        WeakReferenceMessenger.Default.Register<NetworkConnectionsChangedMessage>(this, (_, _) => OnPropertyChanged(nameof(Connections)));

        // returns the active match groups of this tournament. We use the Guid as a token
        // to ensure we only respond to requests for this tournament
        WeakReferenceMessenger.Default.Register<RequestActiveMatchGroups, Guid>(this, Guid,
            (r, m) =>
            {
                MatchGroupsState result = new();

                var bracket = FindStage<BracketStageViewModel>();
                if (bracket is not null)
                {
                    result = new()
                    {
                        Type = "Bracket",
                        Groups = [.. bracket.GetMatchGroupsState()]
                    };
                }
                else
                {
                    var pools = FindStage<PoolsStageViewModel>();
                    if (pools is not null)
                    {
                        result = new()
                        {
                            Type = "Pool",
                            Groups = [.. pools.Pools.Select(p => p.ToState())]
                        };
                    }
                }

                m.Reply(result);
            }
        );
    }

    #endregion
}
