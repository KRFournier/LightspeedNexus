using LightspeedNexus.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

public partial class FighterViewModel : ViewModelBase
{
    #region Model

    private Fighter _fighter = null!;
    public Fighter Model
    {
        get => _fighter;
        set
        {
            SetProperty(ref _fighter, value);
            OnPropertyChanged(nameof(OnlineId));
            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
            OnPropertyChanged(nameof(FullName));
            OnPropertyChanged(nameof(Club));
            Ratings.Clear();
            Ratings.Add(new WeaponRatingViewModel() { Rating = _fighter.Rey });
            Ratings.Add(new WeaponRatingViewModel() { Rating = _fighter.Ren });
            Ratings.Add(new WeaponRatingViewModel() { Rating = _fighter.Tano });
        }
    }

    public string? OnlineId
    {
        get => Model?.OnlineId.ToString();
        set
        {
            if (int.TryParse(value, out var intValue))
            {
                if (Model.OnlineId != intValue)
                {
                    Model.OnlineId = intValue;
                    OnPropertyChanged(nameof(OnlineId));
                }
            }
        }
    }

    public string FirstName
    {
        get => Model.FirstName;
        set
        {
            if (Model.FirstName != value)
            {
                Model.FirstName = value;
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public string LastName
    {
        get => Model.LastName;
        set
        {
            if (Model.LastName != value)
            {
                Model.LastName = value;
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public string? Club
    {
        get => Model.Club;
        set
        {
            if (Model.Club != value)
            {
                Model.Club = value;
                OnPropertyChanged(nameof(Club));
            }
        }
    }

    #endregion

    public FighterViewModel()
    {
        Model = new Fighter();
    }

    public string FullName => $"{LastName}, {FirstName}";

    public ObservableCollection<WeaponRatingViewModel> Ratings { get; set; } = [];
}
