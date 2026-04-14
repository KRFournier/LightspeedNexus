using Avalonia.Controls;
using Avalonia.Input;
using Lightspeed.ViewModels;
using LightspeedNexus.Services;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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

    public async void Match_Tapped(object? sender, TappedEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.None && sender is Decorator border && border.DataContext is MatchViewModel match)
        {
            if (match.First is not null && match.Second is not null)
            {
                var editViewModel = new MatchEditViewModel()
                {
                    First = [new() { Name = match.First.Participant.Name, Points = match.First.Points }],
                    Second = [new() { Name = match.Second.Participant.Name, Points = match.Second.Points }]
                };
                var result = await match.EditDialog(editViewModel, "Set Final Match Score");
                if (result.IsOk)
                {
                    match.UpdateMatch(result.Item.First[0].Points, result.Item.Second[0].Points);
                    App.Services.GetRequiredService<StorageService>().WriteMatch(match.ToModel());
                }
            }
        }
    }

    public async void Match_DoubleTapped(object? sender, TappedEventArgs e)
    {
#if DEBUG
        if (e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift) &&sender is Decorator border && border.DataContext is MatchViewModel match)
        {
            if (match.First is not null && match.Second is not null)
            {
                var (first, second) = ScoreGenerator.GenerateScores(match);
                match.UpdateMatch(first, second);
                App.Services.GetRequiredService<StorageService>().WriteMatch(match.ToModel());
                e.Handled = true;
            }
        }
#endif
    }

    private void PoolsScrollViewer_DoubleTapped(object? sender, TappedEventArgs e)
    {
#if DEBUG
        if (e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift) && sender is ScrollViewer && DataContext is PoolsStageViewModel poolStage)
        {
            foreach (var pool in poolStage.Pools)
            {
                foreach (var currMatch in pool.MatchGroup.Matches)
                {
                    var (first, second) = ScoreGenerator.GenerateScores(currMatch);
                    currMatch.UpdateMatch(first, second);
                    App.Services.GetRequiredService<StorageService>().WriteMatch(currMatch.ToModel());
                }
            }
            return;
        }
#endif
    }
}