using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Reactive;
using System;

namespace LightspeedNexus.Controls;

public partial class MatchSettings : UserControl
{
    public static readonly StyledProperty<int> ScoreProperty =
        AvaloniaProperty.Register<MatchSettings, int>(
            nameof(Score),
            defaultValue: 12,
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceScore);

    public static readonly StyledProperty<TimeSpan> TimeProperty =
        AvaloniaProperty.Register<MatchSettings, TimeSpan>(
            nameof(Time),
            defaultValue: TimeSpan.FromSeconds(90),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceTime);

    public int Score
    {
        get => GetValue(ScoreProperty);
        set => SetValue(ScoreProperty, value);
    }

    public TimeSpan Time
    {
        get => GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public MatchSettings()
    {
        InitializeComponent();

        // Subscribe to property changes to update UI
        this.GetObservable(ScoreProperty).Subscribe(new AnonymousObserver<int>(value => OnScoreChanged(value)));
        this.GetObservable(TimeProperty).Subscribe(new AnonymousObserver<TimeSpan>(value => OnTimeChanged(value)));

        // Initialize UI with default values
        OnScoreChanged(Score);
        OnTimeChanged(Time);
    }

    private static int CoerceScore(AvaloniaObject instance, int value) =>
        Math.Max(0, value);

    private static TimeSpan CoerceTime(AvaloniaObject instance, TimeSpan value) =>
        value.TotalSeconds < 15 ? TimeSpan.FromSeconds(15) : value;

    private void OnScoreChanged(int newScore) => ScoreTextBlock?.Text = $"{newScore} pts";

    private void OnTimeChanged(TimeSpan newTime) => TimeTextBlock?.Text = $"{newTime:m\\:ss}";

    private void Decrease_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int chunk = Score / 4;
        if (chunk-- > 0)
        {
            Score = chunk * 4;
            Time = TimeSpan.FromSeconds(chunk * 30);
        }
    }

    private void Increase_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        int chunk = Score / 4 + 1;
        Score = chunk * 4;
        Time = TimeSpan.FromSeconds(chunk * 30);
    }

    private void IncreaseScore_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Score++;

    private void DecreaseScore_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Score--;

    private void IncreaseTime_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Time += TimeSpan.FromSeconds(15);

    private void DecreaseTime_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Time -= TimeSpan.FromSeconds(15);
}