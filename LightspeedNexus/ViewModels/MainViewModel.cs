using Avalonia.Animation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class MainViewModel : ViewModelBase,
    IRecipient<NavigatePageMessage>, IRecipient<NavigateHomeMessage>,
    IRecipient<BeginWaitMessage>, IRecipient<EndWaitMessage>
{
    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; } = new HomeViewModel();

    [ObservableProperty]
    public partial string? WaitMessage { get; set; }

    public MainViewModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    #region Message Handlers

    public void Receive(NavigatePageMessage message)
    {
        if (CurrentPage is IDisposable d)
            d.Dispose();

        CurrentPage = message.Page;
    }

    public void Receive(NavigateHomeMessage message)
    {
        if (CurrentPage is IDisposable d)
            d.Dispose();

        CurrentPage = new HomeViewModel();
    }

    public void Receive(BeginWaitMessage message) => WaitMessage = message.Value;

    public void Receive(EndWaitMessage message) => WaitMessage = null;

    #endregion

    #region Saber Sports

    [ObservableProperty]
    private bool _showLogin = false;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _password;

    [ObservableProperty]
    private string? _message;

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

    [RelayCommand]
    private void AccepMessage()
    {
        Message = null;
    }

    #endregion
}
