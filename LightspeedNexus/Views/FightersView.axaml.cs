using Avalonia.Controls;
using Avalonia.Interactivity;
using LightspeedNexus.ViewModels;
using LightspeedNexus.Controls;

namespace LightspeedNexus.Views;

public partial class FightersView : UserControl
{
    public FightersView()
    {
        InitializeComponent();
    }

    public void LightspeedBorder_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is LightspeedBorder border && border.DataContext is FighterViewModel fighter)
        {
            if (this.DataContext is FightersViewModel vm && vm.EditFighterCommand.CanExecute(fighter))
                vm.EditFighterCommand.Execute(fighter);
        }
    }
}