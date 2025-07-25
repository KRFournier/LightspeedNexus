using CommunityToolkit.Mvvm.Input;
using LightspeedNexus.Models;
using System;

namespace LightspeedNexus.ViewModels;

public partial class WeaponRatingViewModel : ViewModelBase
{
    private WeaponRating _rating = new();

    public WeaponRating Rating
    {
        get => _rating;
        set
        {
            SetProperty(ref _rating, value);
            OnPropertyChanged(nameof(Class));
            OnPropertyChanged(nameof(Rank));
        }
    }

    public string Class
    {
        get => Rating.Class.ToString();
        set
        {
            if(Enum.TryParse<WeaponClass>(value, out var weaponClass) && Rating.Class != weaponClass)
            {
                Rating.Class = weaponClass;
                OnPropertyChanged(nameof(Class));
            }
        }
    }

    public string Rank
    {
        get => Rating.Rank.ToString();
        set
        {
            if (Rating.Rank != value)
            {
                Rating.Rank = value;
                OnPropertyChanged(nameof(Rank));
            }
        }
    }

    [RelayCommand]
    private void IncrementRank()
    {
        Rating.Rank++;
        OnPropertyChanged(nameof(Rank));
    }

    [RelayCommand]
    private void DecrementRank()
    {
        Rating.Rank--;
        OnPropertyChanged(nameof(Rank));
    }
}