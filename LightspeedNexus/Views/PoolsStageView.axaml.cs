using Avalonia.Controls;
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
}