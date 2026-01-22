using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    public ObservableCollection<Tournament> RecentTournaments { get; set; } = [];

    public bool HasTournaments { get; set; } = false;

    public HomeViewModel()
    {
        if (!Design.IsDesignMode)
        {
            HasTournaments = StorageService.CountTournaments() > 0;
            RecentTournaments = [.. StorageService.ReadRecentTournaments()];
        }
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
    private static void GotoAllTournaments() => WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new TournamentsViewModel()));

    [RelayCommand]
    private void DeleteTournament(Tournament tournament)
    {
        RecentTournaments.Remove(tournament);
        StorageService.Delete<Tournament>(tournament.Id);
    }

    [RelayCommand]
    private async Task ImportFightersFirstTime()
    {
        BeginWait("Importing Fighters from saber-sport.com...");
        (var success, var message, var fighters) = await SaberSportsService.GetAllFighters();
        EndWait();

        if (success)
        {
            foreach (var fighter in fighters)
                StorageService.Write(fighter);
        }
        else
            MessageBox(message);
    }
}
