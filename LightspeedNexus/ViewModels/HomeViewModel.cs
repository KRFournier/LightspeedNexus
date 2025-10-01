using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [RelayCommand]
    private static void GotoNewCompetition()
    {
        WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new CompetitionViewModel()));
    }

    [RelayCommand]
    private static void GotoTournament()
    {
        WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new TournamentViewModel()));
    }

    [RelayCommand]
    private static void GotoFighters()
    {
        WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new FightersViewModel()));
    }
}
