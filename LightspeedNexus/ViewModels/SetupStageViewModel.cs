using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Styling;
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
using System.Text;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// The tournament settings
/// </summary>
public partial class SetupStageViewModel : StageViewModel, IComparer
{
    #region Properties

    [ObservableProperty]
    public partial DateTime? Date { get; set; } = null;

    [ObservableProperty]
    public partial GameMode GameMode{ get; set; } = GameMode.Standard;
    public static string[] GameModes => Enum.GetNames<GameMode>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial Demographic Demographic { get; set; } = Demographic.All;
    public static string[] Demographics => Enum.GetNames<Demographic>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial SkillLevel SkillLevel { get; set; } = SkillLevel.Open;
    public static string[] SkillLevels => Enum.GetNames<SkillLevel>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool ReyAllowed { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool RenAllowed { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool TanoAllowed { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial string? SubTitle { get; set; } = null;

    public ObservableCollection<PlayerViewModel> Players { get; set; } = [];

    #endregion

    /// <summary>
    /// Creates brand new settings
    /// </summary>
    public SetupStageViewModel()
    {
        LoadFighters();
        SortedPlayers = new(Players);
        SortedPlayers.SortDescriptions.Add(new DataGridComparerSortDescription(this, ListSortDirection.Ascending));
    }

    /// <summary>
    /// Loads settings from a model
    /// </summary>
    public SetupStageViewModel(SetupStage model)
    {
        Date = model.Date;
        GameMode = model.GameMode;
        Demographic = model.Demographic;
        SkillLevel = model.SkillLevel;
        ReyAllowed = model.ReyAllowed;
        RenAllowed = model.RenAllowed;
        TanoAllowed = model.TanoAllowed;
        SubTitle = model.SubTitle; 
        
        LoadFighters();
        SortedPlayers = new(Players);
        SortedPlayers.SortDescriptions.Add(new DataGridComparerSortDescription(this, ListSortDirection.Ascending));
    }

    /// <summary>
    /// Converts into a model
    /// </summary>
    public override SetupStage ToModel() => new(
        Name, Date, GameMode, Demographic,
        SkillLevel, RenAllowed, RenAllowed, TanoAllowed, SubTitle
        );

    /// <summary>
    /// The name of the tournament, e.g., Open Rey
    /// </summary>
    public string Title
    {
        get
        {
            StringBuilder sb = new();

            if (Demographic != Demographic.All)
            {
                sb.Append(Demographic.ToString());
                sb.Append("'s ");
            }

            sb.Append(SkillLevel.ToString());

            if (GameMode != GameMode.Standard)
            {
                sb.Append(' ');
                sb.Append(GameMode.ToString());
            }

            if (ReyAllowed && RenAllowed && TanoAllowed)
                sb.Append(" Mixed Weapons");
            else if (ReyAllowed && RenAllowed)
                sb.Append(" Rey/Ren");
            else if (ReyAllowed && TanoAllowed)
                sb.Append(" Rey/Tano");
            else if (RenAllowed && TanoAllowed)
                sb.Append(" Ren/Tano");
            else if (ReyAllowed)
                sb.Append(" Rey");
            else if (RenAllowed)
                sb.Append(" Ren");
            else if (TanoAllowed)
                sb.Append(" Tano");

            if (!string.IsNullOrEmpty(SubTitle))
            {
                sb.Append($" - ");
                sb.Append(SubTitle);
            }

            return sb.ToString();
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
                FighterNames.Add(f.Name);
                FighterLookup[f.Name] = f;
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
        Players.Add(player);
        return player;
    }

    /// <summary>
    /// Effective Rank, when enabled, adjusts a player's rank based on their weapon of choice and their
    /// highest weapon rank.
    /// </summary>
    [ObservableProperty]
    public partial bool UseEffectiveRank { get; set; } = false;
    partial void OnUseEffectiveRankChanged(bool value)
    {
        foreach(var p in Players)
            p.UseEffectiveRank = value;
    }

    #endregion
}
