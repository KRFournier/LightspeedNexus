using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using LightspeedNexus.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

#region Messages

public sealed class RequestPoolsMessage : RequestMessage<PoolsStageViewModel> { }

#endregion

public partial class PoolsStageViewModel : StageViewModel, IRecipient<RequestPoolsMessage>
{
    #region Properties

    public ObservableCollection<PoolViewModel> Pools { get; set; } = [];

    public bool IsStarted => Pools.Any(p => p.MatchGroup.IsStarted);

    public bool IsCompleted => Pools.All(p => p.MatchGroup.IsCompleted);

    #endregion

    #region Commands

    /// <summary>
    /// Go to the Pools Stage
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoToResults))]
    private void GoToResults() => Next = SeedingStageViewModel.FromPools(this);
    public bool CanGoToResults() => IsCompleted;

    #endregion

    #region Message Handlers

    public void Receive(RequestPoolsMessage message) => message.Reply(this);

    #endregion

    public PoolsStageViewModel() : base("Pools")
    {
        StrongReferenceMessenger.Default.RegisterAll(this);
    }

    public override PoolsStage ToModel() => new()
    {
        Pools = [.. Pools.Select(p => p.ToModel())],
        Next = Next?.ToModel()
    };

    public static PoolsStageViewModel FromModel(PoolsStage model)
    {
        var stage = new PoolsStageViewModel();
        foreach (var pool in model.Pools)
            stage.AddPool(PoolViewModel.FromModel(pool));
        stage.Next = FromModel(model.Next);
        return stage;
    }

    public static PoolsStageViewModel FromSquadrons(IEnumerable<SquadronViewModel> sqaudrons)
    {
        var stage = new PoolsStageViewModel();
        foreach (var squadron in sqaudrons)
            stage.AddPool(PoolViewModel.FromSquadron(squadron));
        return stage;
    }

    protected void AddPool(PoolViewModel pool)
    {
        Pools.Add(pool);
        pool.MatchGroup.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MatchGroupViewModel.IsStarted))
                OnPropertyChanged(nameof(IsStarted));
            else if (e.PropertyName == nameof(MatchGroupViewModel.IsCompleted))
            {
                OnPropertyChanged(nameof(IsCompleted));
                GoToResultsCommand.NotifyCanExecuteChanged();
            }
        };
        OnPropertyChanged(nameof(IsStarted));
        OnPropertyChanged(nameof(IsCompleted));
        GoToResultsCommand.NotifyCanExecuteChanged();
    }

    public override void OnTournamentSaved()
    {
        foreach (var pool in Pools)
            pool.MatchGroup.Save();
        Next?.OnTournamentSaved();
    }

    protected override void OnGoingBack()
    {
        foreach (var pool in Pools)
            pool.MatchGroup.PermanentlyDeleteAll();
    }
}
