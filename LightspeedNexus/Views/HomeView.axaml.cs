using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.ViewModels;

namespace LightspeedNexus.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }

    public void RecentTournament_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is Control control && control.DataContext is Tournament t)
            WeakReferenceMessenger.Default.Send(new NavigatePageMessage(new TournamentViewModel(t)));
    }
}