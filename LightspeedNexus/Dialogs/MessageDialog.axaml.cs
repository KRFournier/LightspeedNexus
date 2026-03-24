using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace LightspeedNexus.Dialogs;

public partial class MessageDialog : UserControl
{
    private readonly System.Action? _closeAction;

    public MessageDialog()
    {
        InitializeComponent();
    }

    public MessageDialog(string message, System.Action onClose) : this()
    {
        MessageTextBlock.Text = message;
        _closeAction = onClose;
    }

    private void OK_Click(object? sender, RoutedEventArgs e) => _closeAction?.Invoke();

    private void UserControl_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Escape)
        {
            OK_Click(sender, new RoutedEventArgs());
        }
    }
}