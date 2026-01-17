using CommunityToolkit.Mvvm.Input;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LightspeedNexus.ViewModels;

public partial class PoolsStageViewModel : StageViewModel
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

    public PoolsStageViewModel() : base("Pools")
    {
    }

    public override PoolsStage ToModel() => new()
    {
        Pools = [..Pools.Select(p => p.ToModel())],
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
