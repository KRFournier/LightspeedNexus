using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;
using Avalonia;
using System;
using System.Globalization;
using Avalonia.Media.TextFormatting;
using Avalonia.Controls.Documents;

namespace LightspeedNexus.Controls;

/// <summary>
/// A custom border for Lightspeed Nexus.
/// </summary>
public class TimeBar : Panel
{
    public static readonly StyledProperty<TimeOnly> StartProperty =
        AvaloniaProperty.Register<SchedulePanel, TimeOnly>(nameof(Start), new TimeOnly(8, 0));

    public static readonly StyledProperty<TimeOnly> EndProperty =
        AvaloniaProperty.Register<SchedulePanel, TimeOnly>(nameof(End), new TimeOnly(20, 0));

    public TimeOnly Start
    {
        get => GetValue(StartProperty);
        set => SetValue(StartProperty, value);
    }

    public TimeOnly End
    {
        get => GetValue(EndProperty);
        set => SetValue(EndProperty, value);
    }

    public TimeBar()
    {
        Populate();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StartProperty || change.Property == EndProperty)
        {
            Populate();
        }
    }

    protected void Populate()
    {
        Children.Clear();
        for (int i = Start.Hour; i <= End.Hour; i++)
        {
            Children.Add(new TextBlock()
            {
                Text = TimeOnly.MinValue.AddHours(i).ToString("h:mm tt"),
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var desiredSize = new Size(0, 0);

        // Tell all children to measure themselves, but don't give them a fixed size yet.
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            if (child.DesiredSize.Width > desiredSize.Width)
                desiredSize = desiredSize.WithWidth(child.DesiredSize.Width);
            if (child.DesiredSize.Height > desiredSize.Height)
                desiredSize = desiredSize.WithHeight(child.DesiredSize.Height);

        }

        // Return the desired size of the panel, which is the available space.
        return desiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double blockHeight = finalSize.Height / (Children.Count - 1);
        double y = -(blockHeight / 2.0);
        foreach (var child in Children)
        {
            child.Arrange(new Rect(0, y, finalSize.Width, blockHeight));
            y += blockHeight;
        }

        return finalSize;
    }
}