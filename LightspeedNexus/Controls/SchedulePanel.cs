using Avalonia.Controls;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using System.Collections;
using Avalonia.Controls.Presenters;

namespace LightspeedNexus.Controls;

public class SchedulePanel : Panel
{
    // Main properties for the schedule panel's timeframe
    public static readonly StyledProperty<DateOnly> DateProperty =
        AvaloniaProperty.Register<SchedulePanel, DateOnly>(nameof(Date), DateOnly.FromDateTime(DateTime.Today));

    public static readonly StyledProperty<int> DaysProperty =
        AvaloniaProperty.Register<SchedulePanel, int>(nameof(Days), 1);

    public static readonly StyledProperty<TimeOnly> TimeStartProperty =
        AvaloniaProperty.Register<SchedulePanel, TimeOnly>(nameof(TimeStart), new TimeOnly(8, 0));

    public static readonly StyledProperty<TimeOnly> TimeEndProperty =
        AvaloniaProperty.Register<SchedulePanel, TimeOnly>(nameof(TimeEnd), new TimeOnly(20, 0));

    // Attached properties for child controls (events)
    public static readonly AttachedProperty<int> DayProperty =
        AvaloniaProperty.RegisterAttached<Control, int>("Day", typeof(SchedulePanel));

    public static readonly AttachedProperty<TimeOnly> TimeProperty =
        AvaloniaProperty.RegisterAttached<Control, TimeOnly>("Time", typeof(SchedulePanel));

    public static readonly AttachedProperty<TimeSpan> DurationProperty =
        AvaloniaProperty.RegisterAttached<Control, TimeSpan>("Duration", typeof(SchedulePanel));

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

    public TimeOnly TimeStart
    {
        get => GetValue(TimeStartProperty);
        set => SetValue(TimeStartProperty, value);
    }

    public TimeOnly TimeEnd
    {
        get => GetValue(TimeEndProperty);
        set => SetValue(TimeEndProperty, value);
    }

    public static int GetDay(Control control) => control.GetValue(DayProperty);
    public static void SetDay(Control control, int value) => control.SetValue(DayProperty, value);

    public static TimeOnly GetTime(Control control) => control.GetValue(TimeProperty);
    public static void SetTime(Control control, TimeOnly value) => control.SetValue(TimeProperty, value);

    public static TimeSpan GetDuration(Control control) => control.GetValue(DurationProperty);
    public static void SetDuration(Control control, TimeSpan value) => control.SetValue(DurationProperty, value);

    protected override Size MeasureOverride(Size availableSize)
    {
        // Tell all children to measure themselves, but don't give them a fixed size yet.
        foreach (var child in Children)
        {
            child.Measure(availableSize);
        }

        // Return the desired size of the panel, which is the available space.
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (var child in Children)
        {
            Control ctrl = (child is ContentPresenter cp && cp.Child is not null) ? cp.Child : child;

            double width = Bounds.Width / Days;
            double height = GetDuration(ctrl).TotalHours / (TimeEnd - TimeStart).TotalHours * Bounds.Height;

            double day = GetDay(ctrl);
            double x = width * day;

            double start = (GetTime(ctrl) - TimeStart).TotalMinutes;
            double y = Bounds.Height / (TimeEnd - TimeStart).TotalMinutes * start;

            if (width > 0 && height > 0)
            {
                child.Arrange(new Rect(x, y, width, height));
                child.IsVisible = true;
            }
            else
                child.IsVisible = false;
        }

        return finalSize;
    }

    private DateTime PointToTime(Point pt)
    {
        // Calculate the day based on the X position
        int dayOffset = (int)(pt.X / Bounds.Width * Days);
        DateTime day = Date.AddDays(dayOffset).ToDateTime(TimeOnly.MinValue);

        // Calculate the time of day based on the Y position
        double minutesInDay = (pt.Y / Bounds.Height) * (TimeEnd - TimeStart).TotalMinutes;
        TimeSpan timeOfDay = TimeStart.ToTimeSpan() + TimeSpan.FromMinutes(minutesInDay);

        return day.Add(timeOfDay);
    }

    private Point TimeToPoint(DateTime time)
    {
        // Calculate the day fraction based on the date
        double day = (time - Date.ToDateTime(TimeOnly.MinValue)).Days;
        double daySize = Bounds.Width / Days;

        // Calculate the time of day fraction
        double minutes = (time.TimeOfDay - TimeStart.ToTimeSpan()).TotalMinutes;
        double minuteSize = Bounds.Height / (TimeEnd - TimeStart).TotalMinutes;

        // Convert to panel coordinates
        return new Point(daySize * day, minuteSize * minutes);
    }

    public Rect PointToRect(Point pt, TimeSpan length)
    {
        DateTime time = PointToTime(pt);

        // Round to nearest 15 minutes
        int minutes = (int)Math.Floor(time.Minute / 15.0) * 15;
        time = new DateTime(time.Year, time.Month, time.Day, time.Hour, minutes, 0);
        Point newPt = TimeToPoint(time);

        // Size
        Size newSize = new(
            Bounds.Width / Days,
            length.TotalHours / (TimeEnd - TimeStart).TotalHours * Bounds.Height
            );

        return new Rect(newPt, newSize);
    }

    public (int Day, TimeOnly Time) PointToAppt(Point pt, TimeSpan length)
    {
        DateTime time = PointToTime(pt);

        // Round to nearest 15 minutes
        int minutes = (int)Math.Floor(time.Minute / 15.0) * 15;
        time = new DateTime(time.Year, time.Month, time.Day, time.Hour, minutes, 0);

        int day = (time - Date.ToDateTime(TimeOnly.MinValue)).Days;

        return (day, TimeOnly.FromDateTime(time));
    }
}
