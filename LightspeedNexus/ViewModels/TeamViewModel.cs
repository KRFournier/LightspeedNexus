using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace LightspeedNexus.ViewModels;

public partial class TeamViewModel : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial int? Seed { get; protected set; }

    public ObservableCollection<ParticipantViewModel> Participants { get; protected set; } = [];

    [ObservableProperty]
    public partial int Points { get; set; } = 0;

    [ObservableProperty]
    public partial int MinorViolations { get; set; } = 0;

    #endregion

    /// <summary>
    /// Creates a brand new score
    /// </summary>
    public TeamViewModel() { }

    /// <summary>
    /// Loads an existing score
    /// </summary>
    public TeamViewModel(Team team, IReadOnlyList<ContestantViewModel> players)
    {
        TeamName = team.Name;
        Seed = team.Seed;
        Points = team.Points;
        Participants = [.. team.Members.Select(Participants => new ParticipantViewModel(Participants, players))];
        Participants.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(IsDisqualified));
        };
    }

    /// <summary>
    /// Creates a team of one player
    /// </summary>
    public TeamViewModel(ContestantViewModel player)
    {
        Participants.Add(new ParticipantViewModel()
        {
            Player = player,
            MinorViolations = 0,
            Score = 0
        });
    }

    /// <summary>
    /// Converts to a <see cref="Team"/>.
    /// </summary>
    public Team ToModel(IList<ContestantViewModel> players) => new(
        TeamName,
        Seed,
        [.. Participants.Select(participant => participant.ToModel(players))],
        Points
    );

    public bool IsDisqualified() => Participants.All(participant => participant.Player.IsDisqualified);

    #region Name

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    private partial string? TeamName { get; set; }

    public string? Name => Participants.Count == 1 ? Participants[0].Player.FullName : TeamName;

    #endregion
}
