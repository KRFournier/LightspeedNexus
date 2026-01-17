using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Views;

public partial class BracketMatchView : UserControl
{
    public BracketMatchView()
    {
        InitializeComponent();
    }

    public void Match_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Decorator border && border.DataContext is MatchViewModel match)
        {
            if (match.EditMatchCommand.CanExecute(null))
                match.EditMatchCommand.Execute(null);
        }
    }
}