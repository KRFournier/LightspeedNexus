using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LightspeedNexus.ViewModels;

public partial class PoolsStageViewModel : StageViewModel
{
    #region Properties

    public ObservableCollection<PoolViewModel> Pools { get; set; } = [];

    #endregion

    public PoolsStageViewModel() : base("Pools")
    {
    }

    public PoolsStageViewModel(IEnumerable<SquadronViewModel> sqaudrons) : this()
    {
        Pools = [.. sqaudrons.Select(s => new PoolViewModel(s))];
    }

    public PoolsStageViewModel(PoolsStage model, IReadOnlyList<SquadronViewModel> squadrons) : this()
    {
        Pools = [.. model.Pools.Select(p => new PoolViewModel(p, squadrons))];
    }

    public override PoolsStage ToModel() => new([.. Pools.Select(p => p.ToModel())], Next?.ToModel());
}
