using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Network;
using Lightspeed.Network.Messages;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;
using LightspeedNexus.Networking;
using LightspeedNexus.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.ViewModels;

#region Messages

/// <summary>
/// Send when it's time to save everything and close this tournament
/// </summary>
public sealed class SaveAndCloseMessage { }

/// <summary>
/// Sent when a view model thinks it's a good idea to save the tournament
/// </summary>
public sealed class SaveMessage { }

#endregion

public partial class TournamentViewModel : ViewModelBase,
    IRecipient<RosterChangedMessage>, IRecipient<BracketRoundCompleted>,
    IRecipient<SaveAndCloseMessage>, IRecipient<SaveMessage>,
    IRecipient<StageChangedMessage>, IRecipient<NetworkConnectionsChangedMessage>, IRecipient<RequestActiveMatchGroups>
{
    private readonly StorageService _storageService;
    private readonly NavigationService _navigationService;

    #region Properties

    /// <summary>
    /// The tournament's unique identifier
    /// </summary>
    public Guid Guid { get; set; } = Guid.NewGuid();

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
        _navigationService.NavigateToHome();
    }

    #endregion

    #region Message Handlers

    public void Receive(RosterChangedMessage _)
    {
        OnPropertyChanged(nameof(IsRanked));
        OnPropertyChanged(nameof(InitialRank));
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(FinalRank));
    }

    public void Receive(BracketRoundCompleted _) => OnPropertyChanged(nameof(FinalRank));

    /// <summary>
    /// Saves the tournament and goes back to the main menu. This is typically sent by the last stage when it determines that the tournament is completed,
    /// but it can be sent by any stage that thinks it's a good idea to save and close.
    /// </summary>
    public void Receive(SaveAndCloseMessage _) => GoHome();

    /// <summary>
    /// Saves the whole tournament. This can be triggered by any stage when it thinks it's a good idea to save, such as after completing a round or making a big change to the roster.
    /// The tournament view model doesn't need to know about the stages to allow them to trigger saves, which is nice for separation of concerns.
    /// </summary>
    public void Receive(SaveMessage _) => Save();

    /// <summary>
    /// Notification that we have moved to a new stage. This is sent by the navigation service whenever we navigate to a new stage, and lets us update the UI to reflect the new stage.
    /// </summary>
    public void Receive(StageChangedMessage message)
    {
        Save();
        OnPropertyChanged(nameof(PreviousStages));
        OnPropertyChanged(nameof(CurrentStage));
        OnPropertyChanged(nameof(Value));
        OnPropertyChanged(nameof(InitialRank));
        OnPropertyChanged(nameof(FinalRank));
    }

    /// <summary>
    /// Notification that the number of network connections has changed.
    /// </summary>
    public void Receive(NetworkConnectionsChangedMessage _) => OnPropertyChanged(nameof(Connections));

    /// <summary>
    /// Handles the network's request for the active match groups of this tournament. We use the Guid as a token to ensure we only respond to requests for this tournament.
    /// This is used by the network service to know which matches to send to connected clients.
    /// </summary>
    public void Receive(RequestActiveMatchGroups message)
    {
        MatchGroupsState result = new();
        var bracket = FindStage<BracketStageViewModel>();
        if (bracket is not null)
        {
            result = new()
            {
                Type = "Bracket",
                Groups = [.. bracket.GetMatchGroupsStates()]
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
        message.Reply(result);
    }

    #endregion

    /// <summary>
    /// Creates a brand new tournament
    /// </summary>
    public TournamentViewModel(IServiceProvider serviceProvider, IMessenger messenger, StorageService storageService, NavigationService navigationService)
        : base(serviceProvider, messenger)
    {
        _storageService = storageService;
        _navigationService = navigationService;

        SetupStage = serviceProvider.GetRequiredService<SetupStageViewModel>();

        messenger.Register<RosterChangedMessage>(this);
        messenger.Register<BracketRoundCompleted>(this);
        messenger.Register<SaveAndCloseMessage>(this);
        messenger.Register<SaveMessage>(this);
        messenger.Register<StageChangedMessage>(this);
        messenger.Register<NetworkConnectionsChangedMessage>(this);
        messenger.Register<RequestActiveMatchGroups, Guid>(this, Guid);
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
        if (SetupStage.MeetsRequirements)
        {
            _storageService.WriteTournament(ToModel());
            SetupStage.OnTournamentSaved();
        }
    }

    #region Value and Rank

    /// <summary>
    /// Determines if the tournament is ranked
    /// </summary>
    public bool IsRanked => SetupStage.GameMode == GameMode.Standard && GradingsChart.IsRankable(SetupStage.Registrees.Count);

    /// <summary>
    /// Indicates which players are eligible for promotion
    /// </summary>
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
    public Grading? GetFinalGrading()
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

    #region Networking

    public static int Connections => NetworkService.Connections;

    public static string? Address => NetworkService.GetIPAddress();

    #endregion
}
