using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LightspeedNexus.ViewModels;

public partial class WeaponRatingViewModel : ViewModelBase
{
    public WeaponRating Model
    {
        get => new(Class, Rank);
        set
        {
            Class = value.Class;
            Rank = value.Rank;
        }
    }

    [ObservableProperty]
    public partial WeaponClass Class { get; set; }

    [ObservableProperty]
    public partial Rank Rank { get; set; }

    [RelayCommand]
    private void IncrementRank() => Rank++;

    [RelayCommand]
    private void DecrementRank() => Rank--;

    public override string ToString() => Rank.ToString();
}