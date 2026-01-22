using Avalonia.Controls;
using Avalonia.Interactivity;
using LightspeedNexus.ViewModels;
using LightspeedNexus.Controls;
using LightspeedNexus.Models;
using Avalonia.Input;

namespace LightspeedNexus.Views;

public partial class TournamentsView : UserControl
{
    public TournamentsView()
    {
        InitializeComponent();
    }

    public void Tournament_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is LightspeedBorder border && border.DataContext is Tournament t)
        {
            if (this.DataContext is TournamentsViewModel vm && vm.GoToTournamentCommand.CanExecute(t))
                vm.GoToTournamentCommand.Execute(t);
        }
    }
}