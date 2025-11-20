using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
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
        Participants.CollectionChanged += OnParticipantsChanged;
    }

    /// <summary>
    /// Loads an existing squadron
    /// </summary>
    public SquadronViewModel(Squadron squadron, IReadOnlyList<ParticipantViewModel> participants)
    {
        Participants = [.. squadron.Players.Select(i => participants[i])];
        Participants.CollectionChanged += OnParticipantsChanged;

        Weight = squadron.Weight;
        Settings = new MatchSettingsViewModel(squadron.MatchSettings);
    }

    /// <summary>
    /// When the participants change, update the number of matches
    /// </summary>
    public void OnParticipantsChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(NumMatches));

    /// <summary>
    /// Converts to a <see cref="Squadron"/>
    /// </summary>
    public Squadron ToModel() => new(
        StrongReferenceMessenger.Default.Send(new RequestParticipantIndicies(Participants)),
        Weight,
        Settings.ToModel()
        );

    /// <summary>
    /// Clears participants and resets weight
    /// </summary>
    public void Clear()
    {
        Participants.Clear();
        Weight = 0;
    }
}
