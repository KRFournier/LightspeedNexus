using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace LightspeedNexus.Views;

public partial class MatchEditView : UserControl
{
    public MatchEditView()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            var firstTextBox = this.GetVisualDescendants()
                .OfType<TextBox>()
                .FirstOrDefault();

            if (firstTextBox != null)
            {
                firstTextBox.SelectAll();
                firstTextBox.Focus();
            }
        });

        //FirstField.AttachedToVisualTree += (_, _) => Dispatcher.UIThread.Post(() => {
        //    FirstField.SelectAll();
        //    FirstField.Focus();
        //    });
    }
}