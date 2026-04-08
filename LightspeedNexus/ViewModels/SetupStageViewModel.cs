using Avalonia.Collections;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Models;
using LightspeedNexus.Services;
using LightspeedNexus.Transitions;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

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
    private readonly StorageService _storageService;

    #region Properties

    public override string Name => "Setup";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
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
    partial void OnSkillLevelChanged(SkillLevel value)
    {
        switch (value)
        {
            case SkillLevel.Open:
                AllowARanks = true;
                AllowBRanks = true;
                AllowCRanks = true;
                AllowDRanks = true;
                AllowERanks = true;
                AllowURanks = true;
                break;
            case SkillLevel.Advanced:
                AllowARanks = true;
                AllowBRanks = true;
                AllowCRanks = true;
                AllowDRanks = false;
                AllowERanks = false;
                AllowURanks = false;
                break;
            case SkillLevel.Novice:
                AllowARanks = false;
                AllowBRanks = false;
                AllowCRanks = false;
                AllowDRanks = true;
                AllowERanks = true;
                AllowURanks = true;
                break;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(HasChoice))]
    public partial bool ReyAllowed { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(HasChoice))]
    public partial bool RenAllowed { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    [NotifyPropertyChangedFor(nameof(HasChoice))]
    public partial bool TanoAllowed { get; set; } = false;

    public bool HasChoice => new bool[] { ReyAllowed, RenAllowed, TanoAllowed }.Count(b => b) > 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial string? EventName { get; set; } = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial string? SubTitle { get; set; } = null;

    public ObservableCollection<RegistreeViewModel> Registrees { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool AllowARanks { get; set; } = true;
    partial void OnAllowARanksChanged(bool value) => ValidateRoster();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool AllowBRanks { get; set; } = true;
    partial void OnAllowBRanksChanged(bool value) => ValidateRoster();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool AllowCRanks { get; set; } = true;
    partial void OnAllowCRanksChanged(bool value) => ValidateRoster();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool AllowDRanks { get; set; } = true;
    partial void OnAllowDRanksChanged(bool value) => ValidateRoster();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool AllowERanks { get; set; } = true;
    partial void OnAllowERanksChanged(bool value) => ValidateRoster();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GoNextCommand))]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial bool AllowURanks { get; set; } = true;
    partial void OnAllowURanksChanged(bool value) => ValidateRoster();

    public ObservableCollection<string> Rings { get; set; }

    #endregion

    #region Next Stage

    public bool MeetsRequirements => Date is not null && Registrees.Count >= 4 && Registrees.All(r => r.MeetsRequirements);

    protected override bool CanGoNext() => MeetsRequirements;

    public string BeginSubText
    {
        get
        {
            if (Date is null)
                return "Set a date.";
            else if (Registrees.Count < 4)
                return $"Add {4 - Registrees.Count} more player{(Registrees.Count == 3 ? "" : "s")}.";
            else if (Registrees.Any(r => !r.MeetsRequirements))
                return "Meet rank requirements.";

            return string.Empty;
        }
    }

    #endregion

    /// <summary>
    /// Creates brand new settings
    /// </summary>
    public SetupStageViewModel(IServiceProvider serviceProvider, IMessenger messenger, NavigationService navigationService, StorageService storageService)
        : base(serviceProvider, messenger, navigationService)
    {
        _storageService = storageService;

        Rings = [.. (_storageService.ReadRings() ?? [])];
        LoadFighters();
        SortedPlayers = new(Registrees);
        SortedPlayers.SortDescriptions.Add(new DataGridComparerSortDescription(this, ListSortDirection.Ascending));

        Registrees.CollectionChanged += (s, e) =>
        {
            ValidateRoster();
            Send(new RosterChangedMessage());
        };
    }

    /// <summary>
    /// Setup can go straight to squadrons if there are no teams, or go to teams if there are
    /// </summary>
    public override IStageTransition GetTransitionToNextStage() => NewTransition<SetupToSquadronsTransition>();

    /// <summary>
    /// Converts into a model
    /// </summary>
    public override SetupStage ToModel() => new()
    {
        Date = Date,
        GameMode = GameMode,
        Demographic = Demographic,
        SkillLevel = SkillLevel,
        ReyAllowed = ReyAllowed,
        RenAllowed = RenAllowed,
        TanoAllowed = TanoAllowed,
        Event = EventName,
        SubTitle = SubTitle,
        Registrees = [.. Registrees.Select(r => r.ToModel())],
        AllowARanks = AllowARanks,
        AllowBRanks = AllowBRanks,
        AllowCRanks = AllowCRanks,
        AllowDRanks = AllowDRanks,
        AllowERanks = AllowERanks,
        AllowURanks = AllowURanks,
        Rings = [.. Rings],
        Next = Next?.ToModel()
    };

    /// <summary>
    /// Called when the tournament is saved
    /// </summary>
    public override void OnTournamentSaved()
    {
        _storageService.WriteRings(Rings);
        base.OnTournamentSaved();
    }

    /// <summary>
    /// The name of the event in which the tournament is held
    /// </summary>
    public override string? Event => base.Event;

    /// <summary>
    /// The name of the tournament, e.g., Open Rey
    /// </summary>
    public override string Title => Tournament.GetTitle(Demographic,
        AllowARanks, AllowBRanks, AllowCRanks, AllowDRanks, AllowERanks, AllowURanks,
        GameMode, ReyAllowed, RenAllowed, TanoAllowed, SubTitle);

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
            foreach (var f in _storageService.ReadAll<Fighter>())
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
                CreateAndAddRegistreeFromFighter(fighter, UseEffectiveRank);
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
            var result = await EditDialog(New<FighterViewModel>(), "New Fighter");
            if (result.IsOk)
            {
                var fighter = result.Item.ToModel();
                CreateAndAddRegistreeFromFighter(fighter, UseEffectiveRank);
                FighterNames.Add(result.Item.FullName);
                FighterNames = [.. FighterNames.Order()];
                FighterLookup[result.Item.FullName] = fighter;
                _storageService.Write(fighter);
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
    private async Task EditPlayer(RegistreeViewModel registree)
    {
        try
        {
            var fighter = New<FighterViewModel>();
            fighter.Guid = registree.Guid;
            fighter.OnlineId = registree.OnlineId;
            fighter.FirstName = registree.FirstName;
            fighter.LastName = registree.LastName;
            fighter.Club = registree.Club;
            fighter.ReyRank = registree.ReyRank;
            fighter.RenRank = registree.RenRank;
            fighter.TanoRank = registree.TanoRank;

            var result = await EditDialog(fighter, "Edit Fighter");
            if (result.IsOk)
            {
                registree.Update(result.Item);
                _storageService.Write(result.Item.ToModel());
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unexpected error editing a fighter: {e}");
        }
    }

    /// <summary>
    /// Removes a registree
    /// </summary>
    [RelayCommand]
    private void RemoveRegistree(RegistreeViewModel item)
    {
        Registrees.Remove(item);
        item.PropertyChanged -= OnRegistreePropertyChanged;
        OnPropertyChanged(nameof(SortedPlayers));
    }

    /// <summary>
    /// Creates a new registree view model for the specified fighter, adds it to the collection of registrees, and updates
    /// squadron assignments as needed.
    /// </summary>
    /// <remarks>The returned RegistreeViewModel is added to the Registrees collection. Squadron assignments are
    /// updated immediately and whenever the registree's rank changes, if automatic updates are enabled.</remarks>
    protected RegistreeViewModel CreateAndAddRegistreeFromFighter(Fighter fighter, bool usesEffectiveRank = false, WeaponClass weaponOfChoice = WeaponClass.Rey)
    {
        var registree = New<RegistreeViewModel>();
        registree.Guid = fighter.Id;
        registree.OnlineId = fighter.OnlineId;
        registree.FirstName = fighter.FirstName;
        registree.LastName = fighter.LastName;
        registree.Club = fighter.Club;
        registree.ReyRank = fighter.Rey;
        registree.RenRank = fighter.Ren;
        registree.TanoRank = fighter.Tano;
        registree.UseEffectiveRank = usesEffectiveRank;
        registree.WeaponOfChoice = weaponOfChoice;

        AddRegistree(registree);

        return registree;
    }

    /// <summary>
    /// Adds a registree. Always add a registree using this method
    /// </summary>
    public void AddRegistree(RegistreeViewModel registree)
    {
        Registrees.Add(registree);
        registree.PropertyChanged += OnRegistreePropertyChanged;
    }

    /// <summary>
    /// Responds to changes in weapon choice so the registree can be revalidated
    /// </summary>
    private void OnRegistreePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is RegistreeViewModel r && e.PropertyName == nameof(RegistreeViewModel.WeaponOfChoice))
        {
            r.Validate(AllowARanks, AllowBRanks, AllowCRanks, AllowDRanks, AllowERanks, AllowURanks);
            GoNextCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(BeginSubText));
            Send(new RosterChangedMessage());
        }
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

    /// <summary>
    /// Checks each registree to see if they meet the rank requirements
    /// </summary>
    public void ValidateRoster()
    {
        foreach (var r in Registrees)
            r.Validate(AllowARanks, AllowBRanks, AllowCRanks, AllowDRanks, AllowERanks, AllowURanks);
        GoNextCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(BeginSubText));
    }

    /// <summary>
    /// Validates a single registree
    /// </summary>
    protected void ValidateRegistree(RegistreeViewModel registree)
    {
        registree.Validate(AllowARanks, AllowBRanks, AllowCRanks, AllowDRanks, AllowERanks, AllowURanks);
        GoNextCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(BeginSubText));
    }
}
