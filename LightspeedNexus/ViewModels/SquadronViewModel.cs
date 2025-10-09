using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace LightspeedNexus.ViewModels;

public partial class SquadronViewModel : ViewModelBase, IDisposable
{
    #region Properties

    public ObservableCollection<PlayerViewModel> Players { get; set; } = [];

    [ObservableProperty]
    public partial int Weight { get; set; } = 0;

    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial IBrush Color { get; set; } = Brushes.Transparent;

    [ObservableProperty]
    public partial LocalMatchSettingsViewModel Settings { get; protected set; }

    public int NumMatches => Players.Count * (Players.Count - 1) / 2;

    #endregion

    /// <summary>
    /// Creates a brand new squadron
    /// </summary>
    public SquadronViewModel(MatchSettingsViewModel globalSettings)
    {
        Settings = new LocalMatchSettingsViewModel(globalSettings);
        Players.CollectionChanged += OnPlayersChanged;
    }

    /// <summary>
    /// Loads an existing squadron
    /// </summary>
    public SquadronViewModel(Squadron squadron, MatchSettingsViewModel globalSettings, IReadOnlyList<PlayerViewModel> fullRoster)
    {
        Settings = new LocalMatchSettingsViewModel(globalSettings);
        Players = [.. squadron.Players.Select(i => fullRoster[i])];
        Players.CollectionChanged += OnPlayersChanged;
        Weight = squadron.Weight;
    }

    /// <summary>
    /// Necessary to ensure we properly dispose of event handlers
    /// </summary>
    public void Dispose()
    {
        Settings.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// When the players change, update the number of matches
    /// </summary>
    public void OnPlayersChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(NumMatches));

    /// <summary>
    /// Converts to a <see cref="Squadron"/>
    /// </summary>
    public Squadron ToModel(IList<PlayerViewModel> players) => new([.. Players.Select(p => players.IndexOf(p))], Weight);

    /// <summary>
    /// Clears players and resets weight
    /// </summary>
    public void Clear()
    {
        Players.Clear();
        Weight = 0;
    }
}
