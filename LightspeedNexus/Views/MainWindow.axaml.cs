using Avalonia.Controls;
using LightspeedNexus.Networking;

namespace LightspeedNexus.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += (_, _) => NetworkService.Start();
        Closing += (_, _) => NetworkService.Stop();
    }
}
