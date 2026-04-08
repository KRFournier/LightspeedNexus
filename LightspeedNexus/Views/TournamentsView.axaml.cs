using Avalonia.Controls;
using Avalonia.Input;
using Lightspeed.Controls;
using LightspeedNexus.Models;
using LightspeedNexus.ViewModels;

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