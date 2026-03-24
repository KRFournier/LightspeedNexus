using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using LightspeedNexus.Services;
using LightspeedNexus.ViewModels;
using LightspeedNexus.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        SetupServices();

        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void SetupServices()
    {
        StorageService.RegisterSerializers();
        //SaberSportsService.LoadLastUsed();

        // Set up DI
        var serviceCollection = new ServiceCollection();

        // Services
        //serviceCollection.AddSingleton<StorageService>();

        // ViewModels
        //serviceCollection.AddSingleton<MainViewModel>();
        //serviceCollection.AddTransient<CalendarViewModel>();

        Services = serviceCollection.BuildServiceProvider();
    }
}
