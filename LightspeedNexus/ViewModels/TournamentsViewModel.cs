using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.ViewModels;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace LightspeedNexus.ViewModels;

public partial class TournamentsViewModel : ViewModelBase, IComparer
{
    private readonly StorageService _storageService;
    private readonly ActiveTournamentService _activeTournamentService;
    private readonly NavigationService _navigationService;

    #region Properties

    public ObservableCollection<Tournament> Tournaments { get; set; } = [];

    public DataGridCollectionView? IncompleteTournaments { get; }

    public DataGridCollectionView? CompleteTournaments { get; }

    [ObservableProperty]
    public partial Tournament? Current { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;
    partial void OnSearchTextChanged(string value)
    {
        IncompleteTournaments?.Refresh();
        CompleteTournaments?.Refresh();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void ClearSearch() => SearchText = "";

    [RelayCommand]
    private void GoHome() => _navigationService.NavigateToHome();

    [RelayCommand]
    private async Task GoToTournament(Tournament item) => _activeTournamentService.StartLoadedTournament(item);

    [RelayCommand]
    private void DeleteTournament(Tournament tournament)
    {
        Tournaments.Remove(tournament);
        _storageService.Delete<Tournament>(tournament.Id);
        IncompleteTournaments?.Refresh();
        CompleteTournaments?.Refresh();
    }

    #endregion

    /// <summary>
    /// Creates a new Fighters view model that loads all known fighters from storage and sorts them.
    /// </summary>
    public TournamentsViewModel(IServiceProvider serviceProvider, IMessenger messenger,
        NavigationService navigationService, StorageService storageService, ActiveTournamentService activeTournamentService)
        : base(serviceProvider, messenger)
    {
        _storageService = storageService;
        _activeTournamentService = activeTournamentService;
        _navigationService = navigationService;

        if (!Design.IsDesignMode)
        {
            try
            {
                Tournaments = [.. _storageService.ReadAllTournaments()];

                IncompleteTournaments = new(Tournaments.Where(t => !t.IsCompleted));
                IncompleteTournaments.SortDescriptions.Add(new DataGridComparerSortDescription(this, ListSortDirection.Descending));
                IncompleteTournaments.Filter = TournamentFilter;

                CompleteTournaments = new(Tournaments.Where(t => t.IsCompleted));
                CompleteTournaments.SortDescriptions.Add(new DataGridComparerSortDescription(this, ListSortDirection.Descending));
                CompleteTournaments.Filter = TournamentFilter;

            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unexpected error reading all tournaments: {e}");
            }
        }
    }

    /// <summary>
    /// The filter logic for all tournament
    /// </summary>
    protected bool TournamentFilter(object obj)
    {
        if (obj is Tournament t)
        {
            return string.IsNullOrWhiteSpace(SearchText) ||
                   t.SetupStage.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                   (t.SetupStage.SubTitle?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (t.SetupStage.Event?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (t.SetupStage.Date?.ToLongDateString().Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false);
        }
        return false;
    }

    /// <summary>
    /// Implements comparison for use in sorting the collection view.
    /// </summary>
    public int Compare(object? x, object? y)
    {
        if (x is Tournament t1 && t1.SetupStage.Date is not null && y is Tournament t2 && t2.SetupStage.Date is not null)
            return DateTime.Compare(t1.SetupStage.Date.Value, t2.SetupStage.Date.Value);
        else
            throw new ArgumentException("Both parameters must be of type Tournament.");
    }
}
