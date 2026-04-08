using Avalonia.Controls;
using Avalonia.Input;
using Lightspeed.ViewModels;
using LightspeedNexus.Services;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Views;

public partial class BracketMatchView : UserControl
{
    public BracketMatchView()
    {
        InitializeComponent();
    }

    public async void Match_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Decorator border && border.DataContext is MatchViewModel match)
        {
#if DEBUG
            if (e.KeyModifiers == (KeyModifiers.Control | KeyModifiers.Shift))
            {
                var (first, second) = ScoreGenerator.GenerateScores(match);
                match.UpdateMatch(first, second);
                App.Services.GetRequiredService<StorageService>().WriteMatch(match.ToModel());
                e.Handled = true;
                return;
            }
#endif
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
}