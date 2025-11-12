using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace LightspeedNexus.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public ObservableCollection<Tournament> RecentTournaments { get; set; } = [];

    public HomeViewModel()
    {
        if (!Design.IsDesignMode)
            RecentTournaments = [.. StorageService.ReadRecentTournaments()];
    }

    [RelayCommand]
    private static void GotoNewCompetition()
    {
        // WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new CompetitionViewModel()));
    }

    [RelayCommand]
    private static void GotoTournament() => WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new TournamentViewModel()));

    [RelayCommand]
    private static void GotoFighters() => WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new FightersViewModel()));

    [RelayCommand]
    private void DeleteTournament(Tournament tournament)
    {
        RecentTournaments.Remove(tournament);
        StorageService.Delete<Tournament>(tournament.Id);
    }
}
