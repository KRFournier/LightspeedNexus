using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LightspeedNexus.Messages;
using System;

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