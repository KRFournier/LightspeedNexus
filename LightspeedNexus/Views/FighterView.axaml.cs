using Avalonia.Controls;
using Avalonia.Threading;

namespace LightspeedNexus.Views;

public partial class FighterView : UserControl
{
    public FighterView()
    {
        InitializeComponent();

        FirstField.AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            FirstField.SelectAll();
            FirstField.Focus();
        });
    }
}