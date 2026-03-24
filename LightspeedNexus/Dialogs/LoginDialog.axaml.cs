using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Services;

namespace LightspeedNexus.Dialogs;

public partial class LoginDialog : UserControl
{
    private readonly System.Action? _closeAction;

    public LoginDialog()
    {
        InitializeComponent();
    }

    public LoginDialog(System.Action closeAction) : this()
    {
        _closeAction = closeAction;
        EmailTextBox.Text = SaberSportsService.LastEmail;
        PasswordTextBox.Text = SaberSportsService.LastPassword;

        Dispatcher.UIThread.Post(() =>
        {
            EmailTextBox.Focus();
            EmailTextBox.SelectAll();
        }, DispatcherPriority.Input);
    }

    private void OK_Click(object? sender, RoutedEventArgs e) => _closeAction?.Invoke();

    private void Cancel_Click(object? sender, RoutedEventArgs e) => _closeAction?.Invoke();

    private void UserControl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OK_Click(sender, new RoutedEventArgs());
        }
        else if (e.Key == Key.Escape)
        {
            Cancel_Click(sender, new RoutedEventArgs());
        }
    }

    private async void RegisterButton_Click(object? sender, RoutedEventArgs e)
    {
        var email = EmailTextBox.Text?.Trim();

        if (string.IsNullOrEmpty(email))
        {
            WeakReferenceMessenger.Default.Send(new MessageBoxMessage("Please enter a valid email address."));
            return;
        }

        WeakReferenceMessenger.Default.Send(new BeginWaitMessage($"Registering {email}... "));
        var msg = await SaberSportsService.RegisterEmail(email);
        WeakReferenceMessenger.Default.Send(new EndWaitMessage());
        WeakReferenceMessenger.Default.Send(new MessageBoxMessage(msg));
    }

    private async void ResendButton_Click(object? sender, RoutedEventArgs e)
    {
        var email = EmailTextBox.Text?.Trim();

        if (string.IsNullOrEmpty(email))
        {
            WeakReferenceMessenger.Default.Send(new MessageBoxMessage("Please enter a valid email address."));
            return;
        }

        WeakReferenceMessenger.Default.Send(new BeginWaitMessage($"Resending password to {email}... "));
        var msg = await SaberSportsService.ResendPassword(email);
        WeakReferenceMessenger.Default.Send(new EndWaitMessage());
        WeakReferenceMessenger.Default.Send(new MessageBoxMessage(msg));
    }

    private async void RenewButton_Click(object? sender, RoutedEventArgs e)
    {
        var email = EmailTextBox.Text?.Trim();
        var password = PasswordTextBox.Text;

        if (string.IsNullOrEmpty(email))
        {
            WeakReferenceMessenger.Default.Send(new MessageBoxMessage("Please enter a valid email address."));
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            WeakReferenceMessenger.Default.Send(new MessageBoxMessage("Please enter your current password."));
            return;
        }

        WeakReferenceMessenger.Default.Send(new BeginWaitMessage($"Requesting new password for {email}... "));
        var msg = await SaberSportsService.NewPassword(email, password);
        WeakReferenceMessenger.Default.Send(new EndWaitMessage());
        WeakReferenceMessenger.Default.Send(new MessageBoxMessage(msg));
    }
}