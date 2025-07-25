using LightspeedNexus.Models;
using System;

namespace LightspeedNexus.ViewModels;

public partial class CompetitionViewModel : ViewModelBase
{
    #region Model

    private Competition _model = new();
    public Competition Model
    {
        get => _model;
        set
        {
            if (_model != value)
            {
                _model = value;
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(Start));
                OnPropertyChanged(nameof(End));
            }
        }
    }

    public Guid Id => Model.Id;

    public string Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name != value)
            {
                Model.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public DateOnly Start
    {
        get => Model.Start;
        set
        {
            if (Model.Start != value)
            {
                Model.Start = value;
                OnPropertyChanged(nameof(Start));
            }
        }
    }

    public DateOnly End
    {
        get => Model.End;
        set
        {
            if (Model.End != value)
            {
                Model.End = value;
                OnPropertyChanged(nameof(End));
            }
        }
    }

    #endregion
}
