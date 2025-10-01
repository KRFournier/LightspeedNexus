using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;

namespace LightspeedNexus.Views;

public partial class CompetitionView : UserControl
{
    public CompetitionView()
    {
        InitializeComponent();
        FirstField.AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(() => FirstField.Focus());

        WeakReferenceMessenger.Default.Register<ClearAutoCompleteMessage>(this, (_, _) =>
        {
            RosterSearch.Text = "";
        });
    }
}