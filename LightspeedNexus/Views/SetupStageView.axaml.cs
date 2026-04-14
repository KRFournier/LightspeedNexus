using Avalonia.Controls;
using Avalonia.Input;
using Lightspeed.Controls;
using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Views;

public partial class SetupStageView : UserControl
{
    public SetupStageView()
    {
        InitializeComponent();
    }

    public void Player_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is LightspeedBorder border && border.DataContext is RegistreeViewModel registree)
        {
            if (this.DataContext is SetupStageViewModel vm && vm.EditRegistreeCommand.CanExecute(registree))
                vm.EditRegistreeCommand.Execute(registree);
        }
    }
}