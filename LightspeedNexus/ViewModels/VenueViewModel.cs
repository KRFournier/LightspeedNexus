using CommunityToolkit.Mvvm.Input;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class VenueViewModel : ViewModelBase
{
    #region Model

    private Venue _venue = null!;
    public Venue Model
    {
        get => _venue;
        set
        {
            SetProperty(ref _venue, value);
            OnPropertyChanged(nameof(Name));
            Rings = [.. Model.Rings];
        }
    }

    public string Name
    {
        get => Model.Name;
        set
        {
            if (Model.Name != value)
            {
                Model.Name = value;
            }
        }
    }

    public ObservableCollection<string> Rings { get; set; } = [];

    #endregion

    public VenueViewModel()
    {
        Model = new Venue();
    }

    [RelayCommand]
    private void AddRing()
    {
        Rings.Add($"Ring {Rings.Count + 1}");
    }
}
