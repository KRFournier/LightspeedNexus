using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Lightspeed.Network;
using LightspeedNexus.Messages;
using LightspeedNexus.Networking;

namespace LightspeedNexus.ViewModels;

public partial class MainViewModel : ViewModelBase,
    IRecipient<NavigatePageMessage>, IRecipient<NavigateHomeMessage>, IRecipient<RequestOpenTournaments>
{
    [ObservableProperty]
    public partial ViewModelBase CurrentPage { get; set; } = new HomeViewModel();

    public MainViewModel()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    #region Message Handlers

    public void Receive(NavigatePageMessage message)
    {
        if (CurrentPage is IDisposable d)
            d.Dispose();

        CurrentPage = message.Page;
    }

    public void Receive(NavigateHomeMessage message)
    {
        if (CurrentPage is IDisposable d)
            d.Dispose();

        CurrentPage = new HomeViewModel();
    }

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
