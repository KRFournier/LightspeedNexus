using Avalonia.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Services;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentPage = new HomeViewModel();

    [ObservableProperty]
    private ViewModelBase? _dialog = null;

    private OpenDialogMessage? _dialogMessage = null;

    [ObservableProperty]
    private bool _showLogin = false;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _message;

    public MainViewModel()
    {
        WeakReferenceMessenger.Default.Register<NavigatePageMessage>(this, (_, m) =>
        {
            CurrentPage = m.Value;
        });

        WeakReferenceMessenger.Default.Register<OpenDialogMessage>(this, (_, m) =>
        {
            _dialogMessage = m;
            Dialog = m.Initial;
        });
    }

    [RelayCommand]
    private void CancelDialog()
    {
        Dialog = null;
    }

    [RelayCommand]
    private void AcceptDialog()
    {
        if (Dialog is not null)
        {
            _dialogMessage?.Reply(Dialog);
            Dialog = null;
        }
    }

    [RelayCommand]
    private async Task Register()
    {
        if (Email is not null)
            Message = await SaberSportsService.RegisterEmail(Email);
    }

    [RelayCommand]
    private async Task Resend()
    {
        if (Email is not null)
            Message = await SaberSportsService.RegisterEmail(Email);
    }

    [RelayCommand]
    private async Task NewPassword()
    {
        if (Email is not null)
            Message = await SaberSportsService.RegisterEmail(Email);
    }

    [RelayCommand]
    private void CancelLogin()
    {
        ShowLogin = false;
    }

    [RelayCommand]
    private void AcceptLogin()
    {
        ShowLogin = false;
    }
}
