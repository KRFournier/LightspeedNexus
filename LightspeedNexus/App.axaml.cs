using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Services;
using Lightspeed.ViewModels;
using LightspeedNexus.Services;
using LightspeedNexus.Transitions;
using LightspeedNexus.ViewModels;
using LightspeedNexus.Views;
using LightspeedShared;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        SetupServices();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = Services.GetRequiredService<MainViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void SetupServices()
    {
        // Set up DI
        var serviceCollection = new ServiceCollection();

        // Shared stuff
        SharedDependencies.Register(serviceCollection);

        // Services
        serviceCollection.AddSingleton<IMessenger, WeakReferenceMessenger>();
        serviceCollection.AddSingleton<NavigationService>();
        serviceCollection.AddSingleton<ActiveTournamentService>();
        serviceCollection.AddSingleton<IActiveTournamentService>(x => x.GetRequiredService<ActiveTournamentService>());
        serviceCollection.AddSingleton<SquadronService>();
        serviceCollection.AddSingleton<PoolRankingService>();
        serviceCollection.AddTransient<StorageService>();
        serviceCollection.AddTransient<SaberSportsService>();
        serviceCollection.AddTransient<EncryptionService>();
        serviceCollection.AddTransient<LoadingService>();
        serviceCollection.AddTransient<IVibrateService, VibrateService>();

        // ViewModels
        serviceCollection.AddSingleton<MainViewModel>();
        serviceCollection.AddSingleton<MatchNotFoundViewModel>();
        serviceCollection.AddTransient<FightersViewModel>();
        serviceCollection.AddTransient<FighterViewModel>();
        serviceCollection.AddTransient<HomeViewModel>();
        serviceCollection.AddTransient<MatchEditViewModel>();
        serviceCollection.AddTransient<MatchGroupViewModel>();
        serviceCollection.AddTransient<PoolViewModel>();
        serviceCollection.AddTransient<RegistreeViewModel>();
        serviceCollection.AddTransient<SeedViewModel>();
        serviceCollection.AddTransient<SquadronViewModel>();
        serviceCollection.AddTransient<StatisticsViewModel>();
        serviceCollection.AddTransient<TournamentsViewModel>();
        serviceCollection.AddTransient<TournamentViewModel>();
        serviceCollection.AddTransient<WeaponRatingViewModel>();

        // Stage Transitions
        serviceCollection.AddTransient<GoHomeTransition>();
        serviceCollection.AddTransient<SetupToSquadronsTransition>();
        serviceCollection.AddTransient<SquadronsToPoolsTransition>();
        serviceCollection.AddTransient<PoolsToSeedTransition>();
        serviceCollection.AddTransient<SeedToBracketTransition>();
        serviceCollection.AddTransient<BracketToResultsTransition>();

        // Stages
        serviceCollection.AddTransient<BracketStageViewModel>();
        serviceCollection.AddTransient<PoolsStageViewModel>();
        serviceCollection.AddTransient<ResultsStageViewModel>();
        serviceCollection.AddTransient<SeedingStageViewModel>();
        serviceCollection.AddTransient<SetupStageViewModel>();
        serviceCollection.AddTransient<SquadronsStageViewModel>();

        Services = serviceCollection.BuildServiceProvider();
    }
}
