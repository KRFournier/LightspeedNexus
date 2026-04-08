using Avalonia.Controls;
using Avalonia.Input;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();

        AttachedToVisualTree += (s, e) =>
        {
            if (DataContext is HomeViewModel vm && App.Services.GetRequiredService<StorageService>().Count<Fighter>() == 0)
                vm.ImportFightersFirstTimeCommand.Execute(this);
        };
    }

    public void RecentTournament_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Control control && control.DataContext is Tournament t)
            App.Services.GetRequiredService<ActiveTournamentService>().StartLoadedTournament(t);
    }
}