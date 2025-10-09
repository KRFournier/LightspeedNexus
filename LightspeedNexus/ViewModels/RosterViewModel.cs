using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

public partial class RosterViewModel : ViewModelBase, IComparer
{
    #region Properties

    public ObservableCollection<PlayerViewModel> Players { get; set; } = [];

    public ObservableCollection<SquadronViewModel> Squadrons { get; set; } = [];

    [ObservableProperty]
    public partial bool IsStarted { get; set; } = false;

    [ObservableProperty]
    public partial bool IsAutoAssigned { get; set; } = true;

    [ObservableProperty]
    public partial SettingsViewModel Settings { get; protected set; }

    #endregion

    /// <summary>
    /// Creates a brand new roster
    /// </summary>
    /// <param name="settings">The SettingsViewModel instance that provides configuration and notifies of property changes. Cannot be null.</param>
    public RosterViewModel(SettingsViewModel settings)
    {
        Settings = settings;
        Settings.PropertyChanged += SettingsChanged;

        LoadFighters();
        SortedPlayers = new(Players);
        SortedPlayers.SortDescriptions.Add(new DataGridComparerSortDescription(this, ListSortDirection.Ascending));
    }

    /// <summary>
    /// Loads an existing roster
    /// </summary>
    /// <param name="roster">The roster loaded from storage.</param>
    /// <param name="settings">The application settings to be used for configuring the view model. Cannot be null.</param>
    public RosterViewModel(Roster roster, SettingsViewModel settings)
    {
        Settings = settings;
        Settings.PropertyChanged += SettingsChanged;

        IsStarted = roster.IsStarted;
        IsAuto = roster.IsAutoAssigned;

        Players = [.. roster.Players.Select(p => p.ToViewModel())];
        Squadrons = [.. roster.Squadrons.Select(s => s.ToViewModel(settings.PoolSettings, Players))];

        LoadFighters();
        SortedPlayers = new(Players);
        SortedPlayers.SortDescriptions.Add(new DataGridComparerSortDescription(this, ListSortDirection.Ascending));

        UpdateSquadrons();
    }

    /// <summary>
    /// Converts to a <see cref="Roster"/>
    /// </summary>
    public Roster ToModel() => new(
        [.. Players.Select(p => p.ToModel())],
        [.. Squadrons.Select(s => s.ToModel(Players))],
        IsStarted, IsAuto
        );

    /// <summary>
    /// Listens for changes in allowed weapons and updates all players' weapons of choice accordingly
    /// </summary>
    /// <remarks>If the allowed weapon types are modified, this method ensures that all players have a valid
    /// weapon selected by updating any invalid selections to an allowed weapon.</remarks>
    protected void SettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        // When the allowed weapon types change, we need to make sure that no players have that weapon selected
        if (e.PropertyName == nameof(Settings.ReyAllowed) ||
            e.PropertyName == nameof(Settings.RenAllowed) ||
            e.PropertyName == nameof(Settings.TanoAllowed))
        {
            List<WeaponClass> allowed = [];
            if (Settings.ReyAllowed) allowed.Add(WeaponClass.Rey);
            if (Settings.RenAllowed) allowed.Add(WeaponClass.Ren);
            if (Settings.TanoAllowed) allowed.Add(WeaponClass.Tano);
            if (allowed.Count > 0)
            {
                // for each player set on an unallowed weapon, change it to the first weapon allowed
                foreach (var player in Players)
                {
                    if (!allowed.Contains(player.WeaponOfChoice))
                        player.WeaponOfChoice = allowed[0];
                }
            }
        }
    }

    #region Players

    /// <summary>
    /// The sorted collection view of players
    /// </summary>
    public DataGridCollectionView SortedPlayers { get; protected set; }

    /// <summary>
    /// Loads all fighters from persistent storage and populates the Fighters collection and FighterLookup dictionary
    /// with their names and corresponding objects.
    /// </summary>
    /// <remarks>Safe for use in design mode.</remarks>
    private void LoadFighters()
    {
        if (Design.IsDesignMode)
            return;

        try
        {
            foreach (var f in StorageService.ReadAll<Fighter>())
            {
                FighterNames.Add(f.FullName);
                FighterLookup[f.FullName] = f;
            }
            FighterNames = [.. FighterNames.Order()];
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error reading all fighters: {e}");
        }
    }

    /// <summary>
    /// Comparison method used for sorting players
    /// </summary>
    public int Compare(object? x, object? y)
    {
        if (x is PlayerViewModel pvm1 && y is PlayerViewModel pvm2)
            return pvm1.CompareTo(pvm2);
        else
        {
            throw new ArgumentException("Both parameters must be of type PlayerViewModel.");
        }
    }

    /// <summary>
    /// The user's current search text
    /// </summary>
    [ObservableProperty]
    public partial string FighterSearchText { get; set; } = string.Empty;

    /// <summary>
    /// The currently selected fighter from the dropdown
    /// </summary>
    [ObservableProperty]
    public partial string? SelectedFighter { get; set; } = string.Empty;

    /// <summary>
    /// The list of fighter names
    /// </summary>
    public ObservableCollection<string> FighterNames { get; set; } = [];

    /// <summary>
    /// Lookup table that maps names to Fighter objects for quick retrieval
    /// </summary>
    private Dictionary<string, Fighter> FighterLookup { get; set; } = [];

    [RelayCommand]
    private static void GoHome() => WeakReferenceMessenger.Default.Send<NavigateHomeMessage>();

    [RelayCommand]
    private void AddFighter()
    {
        if (SelectedFighter is not null && FighterLookup.TryGetValue(SelectedFighter, out Fighter? fighter) && fighter is not null)
        {
            if (Players.FirstOrDefault(p => p.Guid == fighter.Id) is null)
            {
                CreateAndAddPlayer(fighter);
                SelectedFighter = string.Empty;
                FighterSearchText = string.Empty;
            }
        }
    }

    [RelayCommand]
    private async Task NewFighter()
    {
        try
        {
            var result = await DialogBox(new FighterViewModel(), "New Fighter");
            if (result.IsOk)
            {
                var fighter = result.Item.ToModel();
                CreateAndAddPlayer(fighter);
                FighterNames.Add(result.Item.FullName);
                FighterNames = [.. FighterNames.Order()];
                FighterLookup[result.Item.FullName] = fighter;
                StorageService.Write(fighter);
                SelectedFighter = string.Empty;
                FighterSearchText = string.Empty;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error creating and saving a new fighter: {e}");
        }
    }

    [RelayCommand]
    private static async Task EditPlayer(PlayerViewModel player)
    {
        try
        {
            var result = await DialogBox(player.Clone(), "Edit Fighter");
            if (result.IsOk)
            {
                player.Update(result.Item);
                StorageService.Write(result.Item.ToModel());
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error editing a fighter: {e}");
        }
    }

    /// <summary>
    /// Removes a player
    /// </summary>
    [RelayCommand]
    private void RemovePlayer(PlayerViewModel item)
    {
        Players.Remove(item);
        OnPropertyChanged(nameof(SortedPlayers));
        UpdateSquadrons();
    }

    /// <summary>
    /// The player that is being dragged
    /// </summary>
    [ObservableProperty]
    public partial PlayerViewModel? DraggingPlayer { get; set; }
    partial void OnDraggingPlayerChanged(PlayerViewModel? oldValue, PlayerViewModel? newValue)
    {
        if (oldValue is not null) oldValue.IsDragging = false;
        if (newValue is not null) newValue.IsDragging = true;
    }

    /// <summary>
    /// As the player drags, we drop the dragging player ont the current one
    /// </summary>
    public void DropOnPlayer(PlayerViewModel target)
    {
        if (DraggingPlayer is null || DraggingPlayer == target)
            return;

        int iDrag = -1;
        foreach (var squad in Squadrons)
        {
            iDrag = squad.Players.IndexOf(DraggingPlayer);
            if (iDrag >= 0)
            {
                int iTarget = squad.Players.IndexOf(target);
                if (iTarget >= 0)
                {
                    (squad.Players[iDrag], squad.Players[iTarget]) = (squad.Players[iTarget], squad.Players[iDrag]);
                    return;
                }
                else
                {
                    squad.Players.RemoveAt(iDrag);
                    squad.Weight -= RankWeight(DraggingPlayer.Rank);
                    break;
                }
            }
        }

        // add it before
        if (iDrag >= 0)
        {
            foreach (var squad in Squadrons)
            {
                int i = squad.Players.IndexOf(target);
                if (i >= 0)
                {
                    squad.Players.Insert(i, DraggingPlayer);
                    squad.Weight += RankWeight(DraggingPlayer.Rank);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Creates a new player view model for the specified fighter, adds it to the collection of players, and updates
    /// squadron assignments as needed.
    /// </summary>
    /// <remarks>The returned PlayerViewModel is added to the Players collection. Squadron assignments are
    /// updated immediately and whenever the player's rank changes, if automatic updates are enabled.</remarks>
    protected PlayerViewModel CreateAndAddPlayer(Fighter fighter)
    {
        var player = new PlayerViewModel(fighter);
        player.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PlayerViewModel.Rank) && IsAuto)
                UpdateSquadrons();
        };
        Players.Add(player);
        UpdateSquadrons();
        return player;
    }

    #endregion

    #region Squadrons

    /// <summary>
    /// Some names and colors we can use for squadrons
    /// </summary>
    static readonly (string Name, string Color)[] SquadronNames = [
        ("Red", "Red"),
        ("Blue", "Blue"),
        ("Green", "Green"),
        ("Purple", "Purple"),
        ("Yellow", "Yellow"),
        ("Light", "White"),
        ("Dark", "Gray"),
        ("Fire", "Orange"),
        ("Laser", "Lime"),
        ("Ice", "Teal"),
        ("Nova", "Violet"),
        ("Imperial", "Magenta"),
        ];

    /// <summary>
    /// The max a squadron size can be
    /// </summary>
    public static readonly int MaxSquadronSize = SquadronNames.Length;

    /// <summary>
    /// The minimum squadrons needed to ensure no squadron exceeds the max limit
    /// yet still accounts for all players in the roster
    /// </summary>
    public int MinSquadrons => (int)Math.Ceiling(Players.Count / (double)MaxSquadronSize);

    /// <summary>
    /// The maximum size an automatic squadron can be
    /// </summary>
    const int AutoSquadronSize = 7;

    /// <summary>
    /// Determines if squadrons are automatically determined
    /// </summary>
    [ObservableProperty]
    public partial bool IsAuto { get; set; } = true;
    partial void OnIsAutoChanged(bool value) => UpdateSquadrons();

    /// <summary>
    /// Uses a bin packing algorithm to repopulate the pools in a balanced fashion
    /// </summary>
    private void UpdateSquadrons()
    {
        // auto calculate the squadrons, or base it on the requested number
        int new_count = IsAuto
            ? (int)Math.Ceiling(Players.Count / (double)AutoSquadronSize)
            : Math.Max(Squadrons.Count, MinSquadrons);

        // remove extra squadrons
        while (Squadrons.Count > new_count)
            RemoveSquadron(Squadrons.Count - 1);

        // we shouldn't have more squadrons than we have names to give them
        if (new_count > SquadronNames.Length)
            throw new InvalidOperationException($"Cannot have more than {SquadronNames.Length} squadrons.");

        if (new_count > 0)
        {
            // add or remove squadrons
            for (int i = Squadrons.Count; i < new_count; i++)
                CreateAndAddSquadron();

            // reset members
            foreach (var s in Squadrons)
                s.Clear();

            // place each player into the squadron with the smallest total Value
            RandomlyAssign([.. Players.Where(p => p.Rank == "A")]);
            RandomlyAssign([.. Players.Where(p => p.Rank == "B")]);
            RandomlyAssign([.. Players.Where(p => p.Rank == "C")]);
            RandomlyAssign([.. Players.Where(p => p.Rank == "D")]);
            RandomlyAssign([.. Players.Where(p => p.Rank == "E")]);
            RandomlyAssign([.. Players.Where(p => p.Rank == "U")]);

            // move last player from squadrons that are 2+ larger than the smallest squadrons
            while (Squadrons.Max(s => s.Players.Count) - Squadrons.Min(s => s.Players.Count) > 1)
            {
                var small = Squadrons.MinBy(s => s.Players.Count);
                if (small is not null)
                {
                    var big = Squadrons.MaxBy(s => s.Players.Count);
                    if (big is not null && big.Players.Count > 0)
                    {
                        int w = RankWeight(big.Players[^1].Rank);
                        small.Players.Add(big.Players[^1]);
                        small.Weight += w;
                        big.Weight -= w;
                        big.Players.RemoveAt(big.Players.Count - 1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Assigns given players to the smallest squadron. If multiple squadrons
    /// tie for the smallest, then the player is assigned randomly.
    /// </summary>
    private void RandomlyAssign(IList<PlayerViewModel> players)
    {
        Random r = new();

        // place each player into the squadron with the smallest total Value
        while (players.Count > 0)
        {
            int i = r.Next(players.Count);
            var player = players[i];
            int w = RankWeight(player.Rank);
            var squadron = Squadrons.MinBy(s => s.Weight);
            if (squadron is not null)
            {
                squadron.Players.Add(player);
                squadron.Weight += w;
            }
            players.RemoveAt(i);
        }
    }

    /// <summary>
    /// Switches back to auto
    /// </summary>
    [RelayCommand]
    private void GoAuto()
    {
        IsAuto = true;
        UpdateSquadrons();
    }

    /// <summary>
    /// Gets the given rank's weight
    /// </summary>
    private static int RankWeight(Rank rank)
        => rank.Letter switch { 'A' => 6, 'B' => 5, 'C' => 4, 'D' => 3, 'E' => 2, _ => 1 };

    /// <summary>
    /// This command is used to add a new Item to the List
    /// </summary>
    [RelayCommand]
    private void Refresh() => UpdateSquadrons();

    /// <summary>
    /// Adds a new squadron
    /// </summary>
    [RelayCommand]
    private void AddSquadron()
    {
        if (Squadrons.Count < MaxSquadronSize)
        {
            IsAuto = false;
            CreateAndAddSquadron();
            UpdateSquadrons();
        }
    }

    /// <summary>
    /// Removes the last squadron
    /// </summary>
    [RelayCommand]
    private void RemoveSquadron()
    {
        if (Squadrons.Count > MinSquadrons)
        {
            if (IsAuto)
                IsAuto = false;
            RemoveSquadron(Squadrons.Count - 1);
            UpdateSquadrons();
        }
    }

    /// <summary>
    /// Creates a new squadron using the given index into SquadronNames
    /// </summary>
    private void CreateAndAddSquadron()
    {
        if (Squadrons.Count < MaxSquadronSize)
        {
            Squadrons.Add(new SquadronViewModel(Settings.PoolSettings)
            {
                Name = SquadronNames[Squadrons.Count].Name,
                Color = App.Current?.FindResource($"{SquadronNames[Squadrons.Count].Color}Brush") as IBrush ?? Brushes.Transparent
            });
        }
    }

    /// <summary>
    /// Removes the squadron at the specified index from the collection.
    /// </summary>
    private void RemoveSquadron(int index)
    {
        if (index >= 0 && index < Squadrons.Count)
        {
            Squadrons[index].Dispose();
            Squadrons.RemoveAt(index);
        }
    }

    #endregion
}
