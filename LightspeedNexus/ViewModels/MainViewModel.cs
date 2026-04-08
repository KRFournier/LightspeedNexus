using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Network;
using Lightspeed.Network.Messages;
using Lightspeed.ViewModels;
using LightspeedNexus.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.ViewModels;

public partial class MainViewModel : ViewModelBase,
    IRecipient<NavigatePageMessage>, IRecipient<NavigateHomeMessage>, IRecipient<RequestOpenTournaments>
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; }

    public MainViewModel(IServiceProvider serviceProvider, IMessenger messenger) : base(serviceProvider, messenger)
    {
        _serviceProvider = serviceProvider;
        CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();
        messenger.RegisterAll(this);
    }

    #region Message Handlers

    public void Receive(NavigatePageMessage message) => CurrentPage = message.Page;

    public void Receive(NavigateHomeMessage message) => CurrentPage = _serviceProvider.GetRequiredService<HomeViewModel>();

    public void Receive(RequestOpenTournaments message)
    {
        var reply = new OpenTournaments();

        if (CurrentPage is TournamentViewModel tvm)
        {
            reply.Tournaments = [new OpenTournament()
                {
                    Id = tvm.Guid,
                    Competition = tvm.SetupStage.Event,
                    Name = tvm.SetupStage.Name
                }
            ];
        }

        message.Reply(reply);
    }

    #endregion
}
