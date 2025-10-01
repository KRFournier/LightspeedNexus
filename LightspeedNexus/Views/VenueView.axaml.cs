using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace LightspeedNexus.Views;

public partial class VenueView : UserControl
{
    public VenueView()
    {
        InitializeComponent();

        FirstField.AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(() => FirstField.Focus());
    }
}