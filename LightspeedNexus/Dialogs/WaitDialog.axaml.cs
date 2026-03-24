using Avalonia.Controls;

namespace LightspeedNexus.Dialogs;

public partial class WaitDialog : UserControl
{
    public WaitDialog()
    {
        InitializeComponent();
    }

    public WaitDialog(string? message = null) : this()
    {
        WaitTextBlock.Text = message ?? "Please wait...";
    }
}