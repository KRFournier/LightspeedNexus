using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace LightspeedNexus.ViewModels;

public partial class SquadronViewModel : ViewModelBase
{
    #region Properties

    public ObservableCollection<ParticipantViewModel> Participants { get; set; } = [];

    [ObservableProperty]
    public partial int Weight { get; set; } = 0;

    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial IBrush Color { get; set; } = Brushes.Transparent;

    [ObservableProperty]
    public partial MatchSettingsViewModel Settings { get; set; } = new();

    public int NumMatches => Participants.Count * (Participants.Count - 1) / 2;

    #endregion

    /// <summary>
    /// Creates a brand new squadron
    /// </summary>
    public SquadronViewModel()
    {
        Participants.CollectionChanged += OnPlayersChanged;
    }

    /// <summary>
    /// Loads an existing squadron
    /// </summary>
    public SquadronViewModel(Squadron squadron, MatchSettingsViewModel globalSettings, IReadOnlyList<ParticipantViewModel> participants)
    {
        Participants = [.. squadron.Players.Select(i => participants[i])];
        Participants.CollectionChanged += OnPlayersChanged;
        Weight = squadron.Weight;
    }

    /// <summary>
    /// When the players change, update the number of matches
    /// </summary>
    public void OnPlayersChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(NumMatches));

    /// <summary>
    /// Converts to a <see cref="Squadron"/>
    /// </summary>
    public Squadron ToModel(IList<ParticipantViewModel> players) => new([.. Participants.Select(p => players.IndexOf(p))], Weight);

    /// <summary>
    /// Clears players and resets weight
    /// </summary>
    public void Clear()
    {
        Participants.Clear();
        Weight = 0;
    }
}
