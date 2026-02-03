using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LightspeedNexus.Messages;
using System;

namespace LightspeedNexus.Dialogs;

public partial class MessageDialog : UserControl
{
    private readonly Action? _closeAction;

    public MessageDialog()
    {
        InitializeComponent();
    }

    public MessageDialog(string message, Action onClose) : this()
    {
        MessageTextBlock.Text = message;
        _closeAction = onClose;
    }

    private void OK_Click(object? sender, RoutedEventArgs e)
    {
        _closeAction?.Invoke();
    }

    private void UserControl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Escape)
        {
            OK_Click(sender, new RoutedEventArgs());
        }
    }
}