using Avalonia.Collections;
using Avalonia.Controls;
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

#region Messages

/// <summary>
/// Sent when registrees are added or removed
/// </summary>
public class RosterChangedMessage() { }

#endregion

/// <summary>
/// The tournament settings
/// </summary>
public partial class SetupStageViewModel : StageViewModel, IComparer
{
    #region Properties

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BeginCommand))]
    [NotifyPropertyChangedFor(nameof(BeginSubText))]
    public partial DateTime? Date { get; set; } = null;

    [ObservableProperty]
    public partial GameMode GameMode { get; set; } = GameMode.Standard;
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

    public ObservableCollection<RegistreeViewModel> Registrees { get; set; } = [];

    #endregion

    #region Next Stage

    [RelayCommand(CanExecute = nameof(CanBegin))]
    private void Begin() => Next = new SquadronsStageViewModel(Registrees.Select(r => new PlayerViewModel(r)));

    public bool CanBegin() => Date is not null && Registrees.Count >= 4;

    public string BeginSubText => CanBegin() ?
        "" :
        Date is null ?
            "Set a date" :
            $"Add {4 - Registrees.Count} more player{(Registrees.Count == 3 ? "" : "s")}.";

    #endregion

    /// <summary>
    /// Creates brand new settings
    /// </summary>
    public SetupStageViewModel() : base("Setup")
    {
        LoadFighters();
        SortedPlayers = new(Registrees);
        SortedPlayers.SortDescriptions.Add(new DataGridComparerSortDescription(this, ListSortDirection.Ascending));

        Registrees.CollectionChanged += (s, e) =>
        {
            BeginCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(BeginSubText));
            StrongReferenceMessenger.Default.Send(new RosterChangedMessage());
        };
    }

    /// <summary>
    /// Loads settings from a model
    /// </summary>
    public SetupStageViewModel(SetupStage model) : this()
    {
        Date = model.Date;
        GameMode = model.GameMode;
        Demographic = model.Demographic;
        SkillLevel = model.SkillLevel;
        ReyAllowed = model.ReyAllowed;
        RenAllowed = model.RenAllowed;
        TanoAllowed = model.TanoAllowed;
        SubTitle = model.SubTitle;

        foreach (var r in model.Registrees)
            Registrees.Add(new RegistreeViewModel(r));

        Next = model.Next switch
        {
            SquadronsStage ss => new SquadronsStageViewModel(ss),
            _ => null
        };
    }

    /// <summary>
    /// Converts into a model
    /// </summary>
    public override SetupStage ToModel() => new(
        Date, GameMode, Demographic, SkillLevel, ReyAllowed, RenAllowed,
        TanoAllowed, SubTitle, Registrees.Select(r => r.ToModel()),
        Next?.ToModel());

    /// <summary>
    /// The name of the tournament, e.g., Open Rey
    /// </summary>
    public override string Title => Tournament.GetTitle(Demographic, SkillLevel, GameMode,
        ReyAllowed, RenAllowed, TanoAllowed, SubTitle);

    /// <summary>
    /// Used to update registree settings when tournament settings change
    /// </summary>
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // When the allowed weapon types change, we need to make sure that no players have that weapon selected
        if (e.PropertyName == nameof(ReyAllowed) ||
            e.PropertyName == nameof(RenAllowed) ||
            e.PropertyName == nameof(TanoAllowed))
        {
            List<WeaponClass> allowed = [];
            if (ReyAllowed) allowed.Add(WeaponClass.Rey);
            if (RenAllowed) allowed.Add(WeaponClass.Ren);
            if (TanoAllowed) allowed.Add(WeaponClass.Tano);
            if (allowed.Count > 0)
            {
                // for each player set on an unallowed weapon, change it to the first weapon allowed
                foreach (var registree in Registrees)
                {
                    if (!allowed.Contains(registree.WeaponOfChoice))
                        registree.WeaponOfChoice = allowed[0];
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
        if (x is RegistreeViewModel pvm1 && y is RegistreeViewModel pvm2)
            return pvm1.CompareTo(pvm2);
        else
        {
            throw new ArgumentException("Both parameters must be of type RegistreeViewModel.");
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

    /// <summary>
    /// Registers the <see cref="SelectedFighter"/>
    /// </summary>
    [RelayCommand]
    private void AddFighter()
    {
        if (SelectedFighter is not null && FighterLookup.TryGetValue(SelectedFighter, out Fighter? fighter) && fighter is not null)
        {
            if (Registrees.FirstOrDefault(p => p.Guid == fighter.Id) is null)
            {
                CreateAndAddPlayer(fighter);
                SelectedFighter = string.Empty;
                FighterSearchText = string.Empty;
            }
        }
    }

    /// <summary>
    /// Creates a new fighter and registers them
    /// </summary>
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

    /// <summary>
    /// Edit's a registree's fighter information
    /// </summary>
    [RelayCommand]
    private static async Task EditPlayer(RegistreeViewModel registree)
    {
        try
        {
            var result = await DialogBox(registree.ToFighterViewModel(), "Edit Fighter");
            if (result.IsOk)
            {
                registree.Update(result.Item);
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
    private void RemovePlayer(RegistreeViewModel item)
    {
        Registrees.Remove(item);
        OnPropertyChanged(nameof(SortedPlayers));
    }

    /// <summary>
    /// Creates a new player view model for the specified fighter, adds it to the collection of players, and updates
    /// squadron assignments as needed.
    /// </summary>
    /// <remarks>The returned RegistreeViewModel is added to the Players collection. Squadron assignments are
    /// updated immediately and whenever the player's rank changes, if automatic updates are enabled.</remarks>
    protected RegistreeViewModel CreateAndAddPlayer(Fighter fighter)
    {
        var player = new RegistreeViewModel(new Registree(fighter, UseEffectiveRank));
        Registrees.Add(player);
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
        foreach (var p in Registrees)
            p.UseEffectiveRank = value;
    }

    #endregion
}
