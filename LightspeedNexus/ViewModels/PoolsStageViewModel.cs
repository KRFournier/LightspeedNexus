using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using LightspeedNexus.Transitions;
using System.Collections.ObjectModel;

namespace LightspeedNexus.ViewModels;

public partial class PoolsStageViewModel(IServiceProvider serviceProvider, IMessenger messenger, NavigationService navigationService) : StageViewModel(serviceProvider, messenger, navigationService)
{
    #region Properties

    public override string Name => "Pools";

    public ObservableCollection<PoolViewModel> Pools { get; set; } = [];

    public bool IsStarted => Pools.Any(p => p.MatchGroup.IsStarted);

    public bool IsCompleted => Pools.All(p => p.MatchGroup.IsCompleted);

    #endregion

    protected override bool CanGoNext() => IsCompleted;

    public override IStageTransition GetTransitionToNextStage() => New<PoolsToSeedTransition>();

    public override PoolsStage ToModel() => new()
    {
        Pools = [.. Pools.Select(p => p.ToModel())],
        Next = Next?.ToModel()
    };

    public void AddPool(PoolViewModel pool)
    {
        Pools.Add(pool);
        pool.MatchGroup.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MatchGroupViewModel.IsStarted))
                OnPropertyChanged(nameof(IsStarted));
            else if (e.PropertyName == nameof(MatchGroupViewModel.IsCompleted))
            {
                OnPropertyChanged(nameof(IsCompleted));
                GoNextCommand.NotifyCanExecuteChanged();
            }
        };
        OnPropertyChanged(nameof(IsStarted));
        OnPropertyChanged(nameof(IsCompleted));
        GoNextCommand.NotifyCanExecuteChanged();
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
