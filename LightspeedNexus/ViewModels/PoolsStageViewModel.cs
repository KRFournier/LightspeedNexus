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

    public override PoolsStage ToModel() => new()
    {
        Pools = [..Pools.Select(p => p.ToModel())],
        Next = Next?.ToModel()
    };

    public static PoolsStageViewModel FromModel(PoolsStage model) => new()
    {
        Pools = [.. model.Pools.Select(p => PoolViewModel.FromModel(p))]
    };

    public static PoolsStageViewModel FromSquadrons(IEnumerable<SquadronViewModel> sqaudrons) => new()
    {
        Pools = [.. sqaudrons.Select(s => PoolViewModel.FromSquadron(s))]
    };

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
