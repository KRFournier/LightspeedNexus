using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LightspeedNexus.Controls;
using LightspeedNexus.ViewModels;
using System;

namespace LightspeedNexus.Views;

public partial class PoolsStageView : UserControl
{
    public PoolsStageView()
    {
        InitializeComponent();

        PoolsScrollViewer.PointerWheelChanged += (s, e) =>
        {
            if (e.Delta.Y > 0)
                PoolsScrollViewer.PageLeft();
            else
                PoolsScrollViewer.PageRight();
        };
    }

    public void OnPoolSizeChanged(object? sender, SizeChangedEventArgs args)
    {
        if (sender is StackPanel panel)
        {
            panel.Children[2].Height = Math.Max(0, panel.Bounds.Height - panel.Children[0].Bounds.Height - panel.Children[1].Bounds.Height);
        }
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