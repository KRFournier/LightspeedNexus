using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using System;

namespace LightspeedNexus.Controls;

/// <summary>
/// A custom border for Lightspeed Nexus.
/// </summary>
public class ScheduleGrid : Control
{
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(Foreground), null);

    public static readonly StyledProperty<DateOnly> DateProperty =
        AvaloniaProperty.Register<SchedulePanel, DateOnly>(nameof(Date), DateOnly.FromDateTime(DateTime.Today));

    public static readonly StyledProperty<int> DaysProperty =
        AvaloniaProperty.Register<SchedulePanel, int>(nameof(Days), 1);

    public static readonly StyledProperty<TimeOnly> StartProperty =
        AvaloniaProperty.Register<SchedulePanel, TimeOnly>(nameof(Start), new TimeOnly(8, 0));

    public static readonly StyledProperty<TimeOnly> EndProperty =
        AvaloniaProperty.Register<SchedulePanel, TimeOnly>(nameof(End), new TimeOnly(20, 0));

    /// <summary>
    /// Initializes static members of the <see cref="Border"/> class.
    /// </summary>
    static ScheduleGrid()
    {
        AffectsRender<ScheduleGrid>(
            ForegroundProperty,
            DateProperty,
            DaysProperty,
            StartProperty,
            EndProperty
            );
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public DateOnly Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public int Days
    {
        get => GetValue(DaysProperty);
        set => SetValue(DaysProperty, value);
    }

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

    /// <summary>
    /// Renders the control.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public override void Render(DrawingContext context)
    {
        if (Start > End)
            (Start, End) = (End, Start);

        int startHour = Start.Hour;
        int lastHour = End.Hour;

        var rect = new Rect(0, 0, Bounds.Width, Bounds.Height);
        double dayWidth = Bounds.Size.Width / Days;
        double timeHeight = Bounds.Size.Height / (lastHour - startHour);
        var pen = new Pen(Foreground ?? Brushes.White, 1);
        var dashPen = new Pen(Foreground ?? Brushes.White, 1, dashStyle: DashStyle.Dash);

        // draw time lines
        for(double y = timeHeight; y < rect.Height; y += timeHeight )
            context.DrawLine(pen, new(0, y), new(rect.Width, y));

        // draw dashed lines
        for (double y = timeHeight / 2; y < rect.Height; y += timeHeight)
            context.DrawLine(dashPen, new(0, y), new(rect.Width, y));

        // draw vertical lines
        for (double x = dayWidth; x < rect.Width; x += dayWidth)
            context.DrawLine(pen, new(x, 0), new(x, rect.Height));

        // draw border
        context.DrawRectangle(pen, rect);
    }
}