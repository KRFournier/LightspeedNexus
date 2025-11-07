using Avalonia.Controls;
using Avalonia.Interactivity;
using LightspeedNexus.Controls;
using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Views;

public partial class SetupStageView : UserControl
{
    public SetupStageView()
    {
        InitializeComponent();
    }

    public void Player_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (sender is LightspeedBorder border && border.DataContext is RegistreeViewModel registree)
        {
            if (this.DataContext is SetupStageViewModel vm && vm.EditPlayerCommand.CanExecute(registree))
                vm.EditPlayerCommand.Execute(registree);
        }
    }
}