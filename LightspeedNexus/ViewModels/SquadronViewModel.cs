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
    public partial Guid Guid { get; set; } = Guid.NewGuid();

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
    /// When the participants change, update the number of matches
    /// </summary>
    public void OnParticipantsChanged(object? sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(NumMatches));

    /// <summary>
    /// Converts to a <see cref="Squadron"/>
    /// </summary>
    public Squadron ToModel() => new()
    {
        Guid = Guid,
        Players = [.. Participants.Select(p => p.Guid)],
        Weight = Weight,
        MatchSettings = Settings.ToModel()
    };

    /// <summary>
    /// Converts from a <see cref="Squadron"/>
    /// </summary>
    public static SquadronViewModel FromModel(Squadron squadron) => new()
    {
        Guid = squadron.Guid,
        Participants = [.. squadron.Players.Select(id => StrongReferenceMessenger.Default.Send(new RequestParticipant(id)))],
        Weight = squadron.Weight,
        Settings = MatchSettingsViewModel.FromModel(squadron.MatchSettings)
    };

    /// <summary>
    /// Clears participants and resets weight
    /// </summary>
    public void Clear()
    {
        Participants.Clear();
        Weight = 0;
    }
}
