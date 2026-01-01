using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System;
using System.Diagnostics;

namespace LightspeedNexus.Controls;

/// <summary>
/// Represents a control that raises its <see cref="Button.Click"/> event when it is pressed and held
/// for a length of time.
/// </summary>
public class HoldButton : Button
{
    /// <summary>
    /// Tracks how much time has passed
    /// </summary>
    private Stopwatch _stopwatch = new();

    private const int _interval = 20;

    /// <summary>
    /// Defines the <see cref="Delay"/> property.
    /// </summary>
    public static readonly StyledProperty<int> DelayProperty =
        AvaloniaProperty.Register<HoldButton, int>(nameof(Delay), 2000);

    /// <summary>
    /// Defines the <see cref="Progress"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<HoldButton, double>(nameof(Progress), 0.0);

    private DispatcherTimer? _holdTimer;

    /// <summary>
    /// Gets or sets the amount of time, in milliseconds, to wait before repeating begins.
    /// </summary>
    public int Delay
    {
        get => GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    /// <summary>
    /// Gets or sets the progress of the hold action, from 0.0 to 1.0.
    /// </summary>
    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    private void StartTimer()
    {
        if (_holdTimer == null)
        {
            _holdTimer = new DispatcherTimer();
            _holdTimer.Tick += HoldTimerOnTick;
            _holdTimer.Interval = TimeSpan.FromMilliseconds(_interval);
        }

        if (_holdTimer.IsEnabled) return;

        Progress = 0.0;
        _stopwatch.Restart();
        _holdTimer.Start();
    }

    private void StopTimer()
    {
        Progress = 0.0;
        _holdTimer?.Stop();
    }

    private void HoldTimerOnTick(object? sender, EventArgs e)
    {
        Progress = (double)_stopwatch.ElapsedMilliseconds / Delay;
        if (_stopwatch.ElapsedMilliseconds > Delay)
        {
            OnClick();
            StopTimer();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsPressedProperty && change.GetNewValue<bool>() == false)
        {
            StopTimer();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        ClickMode = ClickMode.Release;
        base.OnKeyDown(e);

        if (e.Key == Key.Space)
        {
            StartTimer();
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        ClickMode = ClickMode.Press;
        base.OnKeyUp(e);

        StopTimer();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        ClickMode = ClickMode.Release;
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            StartTimer();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        ClickMode = ClickMode.Press;
        base.OnPointerReleased(e);

        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            StopTimer();
        }
    }
}

