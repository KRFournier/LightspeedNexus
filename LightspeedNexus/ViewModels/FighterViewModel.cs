using LightspeedNexus.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

public partial class FighterViewModel : ViewModelBase
{
    public FighterViewModel()
    {
        Fighter = new Fighter();
    }

    private Fighter _fighter = null!;
    public Fighter Fighter
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
            Ratings = [.. _fighter.Ratings.Select(r => new WeaponRatingViewModel() { Rating = r })];
        }
    }

    public string? OnlineId
    {
        get => Fighter?.OnlineId.ToString();
        set
        {
            if (int.TryParse(value, out var intValue))
            {
                if (Fighter.OnlineId != intValue)
                {
                    Fighter.OnlineId = intValue;
                    OnPropertyChanged(nameof(OnlineId));
                }
            }
        }
    }

    public string FirstName
    {
        get => Fighter.FirstName;
        set
        {
            if (Fighter.FirstName != value)
            {
                Fighter.FirstName = value;
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public string LastName
    {
        get => Fighter.LastName;
        set
        {
            if (Fighter.LastName != value)
            {
                Fighter.LastName = value;
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public string FullName => $"{LastName}, {FirstName}";

    public string? Club
    {
        get => Fighter.Club;
        set
        {
            if (Fighter.Club != value)
            {
                Fighter.Club = value;
                OnPropertyChanged(nameof(Club));
            }
        }
    }

    public ObservableCollection<WeaponRatingViewModel> Ratings { get; set; } = [];
}
