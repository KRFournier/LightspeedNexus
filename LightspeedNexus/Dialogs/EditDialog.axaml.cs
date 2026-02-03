using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LightspeedNexus.Messages;
using System;

namespace LightspeedNexus.Dialogs;

public partial class EditDialog : UserControl
{
    private readonly EditDialogMessage? _message;
    private readonly Action? _closeAction;

    public EditDialog()
    {
        InitializeComponent();
    }

    public EditDialog(EditDialogMessage message, Action onClose) : this()
    {
        _message = message;
        _closeAction = onClose;

        TitleTextBlock.Text = message.Title;

        var content = new ViewLocator().Build(message.Item);
        content.DataContext = message.Item;
        ContentControl.Content = content;

        // put focus into the first textbox
        foreach (var child in ContentControl.GetVisualChildren())
        {
            if (child is TextBox tb && tb.IsVisible && tb.IsEnabled)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    tb.Focus();
                    tb.SelectAll();
                }, DispatcherPriority.Input);
                break;
            }
        }
    }

    private void OK_Click(object? sender, RoutedEventArgs e)
    {
        _closeAction?.Invoke();
        _message?.Respond(DialogResponse.Ok);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        _closeAction?.Invoke();
        _message?.Respond(DialogResponse.Cancel);
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