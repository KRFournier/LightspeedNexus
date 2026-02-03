using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Services;
using System;
using System.Threading.Tasks;

namespace LightspeedNexus.Dialogs;

public partial class SignDialog : UserControl
{
    private readonly Action<string?>? _closeAction;

    public SignDialog()
    {
        InitializeComponent();
    }

    public SignDialog(Action<string?> closeAction) : this()
    {
        _closeAction = closeAction;
        Dispatcher.UIThread.Post(() => SignTextBox.Focus(), DispatcherPriority.Input);
    }

    private void OK_Click(object? sender, RoutedEventArgs e)
    {
        _closeAction?.Invoke(SignTextBox.Text);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        _closeAction?.Invoke(null);
    }

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
}