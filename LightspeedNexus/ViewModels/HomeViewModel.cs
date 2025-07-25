using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;

namespace LightspeedNexus.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    [RelayCommand]
    private void GotoFighters()
    {
        WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new FightersViewModel()));
    }
}
