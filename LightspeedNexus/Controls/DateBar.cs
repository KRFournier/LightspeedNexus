using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;
using Avalonia;
using System;
using System.Globalization;
using Avalonia.Media.TextFormatting;
using Avalonia.Controls.Documents;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;

namespace LightspeedNexus.Controls;

/// <summary>
/// A custom border for Lightspeed Nexus.
/// </summary>
public class DateBar : Panel
{
    public static readonly StyledProperty<DateOnly> DateProperty =
        AvaloniaProperty.Register<SchedulePanel, DateOnly>(nameof(Date), DateOnly.FromDateTime(DateTime.Today));

    public static readonly StyledProperty<int> DaysProperty =
        AvaloniaProperty.Register<SchedulePanel, int>(nameof(Days), 1);

    public DateOnly Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public int Days
    {
        get => GetValue(DaysProperty);
        set
        {
            if (Days < 1)
                throw new ApplicationException("Days must be at least 1");
            SetValue(DaysProperty, value);
        }
    }

    public DateBar()
    {
        Populate();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DateProperty || change.Property == DaysProperty)
        {
            Populate();
        }
    }

    protected void Populate()
    {
        Children.Clear();
        for (int i = 0; i < Days; i++)
        {
            var date = Date.AddDays(i);
            var panel = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Center };
            panel.Children.Add(new TextBlock()
            {
                Text = date.ToString("dddd"),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            panel.Children.Add(new TextBlock()
            {
                Text = date.ToString("MMMM d"),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
            });
            Children.Add(panel);
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
        double blockWidth = finalSize.Width / Days;
        double x = 0;
        foreach (var child in Children)
        {
            child.Arrange(new Rect(x, 0, blockWidth, finalSize.Height));
            x += blockWidth;
        }

        return finalSize;
    }
}