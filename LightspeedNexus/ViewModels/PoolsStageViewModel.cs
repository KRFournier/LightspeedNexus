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

namespace LightspeedNexus.ViewModels;

public partial class PoolsStageViewModel : StageViewModel
{
    #region Properties

    public ObservableCollection<PoolViewModel> Pools { get; set; } = [];

    public bool IsStarted => Pools.Any(p => p.MatchGroup.IsStarted);

    public bool IsCompleted => Pools.All(p => p.MatchGroup.IsCompleted);

    #endregion

    #region Commands

    [RelayCommand]
    private async static Task EditMatch(MatchViewModel match)
    {
        try
        {
            var result = await DialogBox(match.GetEditViewModel(), "Set Final Match Score");
            if (result.IsOk)
                match.UpdateMatch(result.Item);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error editing a match: {e}");
        }
    }

    #endregion

    public PoolsStageViewModel() : base("Pools")
    {
    }

    protected PoolsStageViewModel(PoolsStage model) : this()
    {
        foreach (var pool in model.Pools)
            AddPool(PoolViewModel.FromModel(pool));
    }

    protected PoolsStageViewModel(IEnumerable<SquadronViewModel> sqaudrons) : this()
    {
        foreach (var squadron in sqaudrons)
            AddPool(PoolViewModel.FromSquadron(squadron));
    }

    public override PoolsStage ToModel() => new()
    {
        Pools = [..Pools.Select(p => p.ToModel())],
        Next = Next?.ToModel()
    };

    public static PoolsStageViewModel FromModel(PoolsStage model) => new(model);

    public static PoolsStageViewModel FromSquadrons(IEnumerable<SquadronViewModel> sqaudrons) => new(sqaudrons);

    protected void AddPool(PoolViewModel pool)
    {
        Pools.Add(pool);
        pool.MatchGroup.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MatchGroupViewModel.IsStarted))
                OnPropertyChanged(nameof(IsStarted));
            else if (e.PropertyName == nameof(MatchGroupViewModel.IsCompleted))
                OnPropertyChanged(nameof(IsCompleted));
        };
        OnPropertyChanged(nameof(IsStarted));
        OnPropertyChanged(nameof(IsCompleted));
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
