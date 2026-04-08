using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System.Collections.ObjectModel;

namespace LightspeedNexus.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly StorageService _storageService;
    private readonly SaberSportsService _saberSportsService;
    private readonly NavigationService _navigationService;
    private readonly ActiveTournamentService _activeTournamentService;

    public ObservableCollection<Tournament> RecentTournaments { get; set; } = [];

    public bool HasTournaments { get; set; } = false;

    public HomeViewModel(IServiceProvider serviceProvider, IMessenger messenger, StorageService storageService, SaberSportsService saberSportsService,
        NavigationService navigationService, ActiveTournamentService activeTournamentService)
        : base(serviceProvider, messenger)
    {
        _storageService = storageService;
        _saberSportsService = saberSportsService;
        _navigationService = navigationService;
        _activeTournamentService = activeTournamentService;

        if (!Design.IsDesignMode)
        {
            HasTournaments = _storageService.CountTournaments() > 0;
            RecentTournaments = [.. _storageService.ReadRecentTournaments()];
        }

        // whenever we come back to the home page, clean up weak reference components
        messenger.Cleanup();
    }

    [RelayCommand]
    private void GotoTournament() => _activeTournamentService.StartNewTournament();

    [RelayCommand]
    private void GotoFighters() => _navigationService.NavigateTo<FightersViewModel>();

    [RelayCommand]
    private void GotoAllTournaments() => _navigationService.NavigateTo<TournamentsViewModel>();

    [RelayCommand]
    private void DeleteTournament(Tournament tournament)
    {
        RecentTournaments.Remove(tournament);
        _storageService.Delete<Tournament>(tournament.Id);
    }

    [RelayCommand]
    private async Task ImportFightersFirstTime()
    {
        BeginWait("Importing Fighters from saber-sport.com...");
        (var success, var message, var fighters) = await _saberSportsService.GetAllFighters();
        EndWait();

        if (success)
        {
            foreach (var fighter in fighters)
                _storageService.Write(fighter);
        }
        else
            await MessageBox(message);
    }
}
