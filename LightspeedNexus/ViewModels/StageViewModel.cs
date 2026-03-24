using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A tournament stage. Base class for all stages
/// </summary>
public abstract partial class StageViewModel(string name) : ViewModelBase, IDisposable
{
    #region Properties

    /// <summary>
    /// The name of this stage
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = name;

    /// <summary>
    /// The next stage of the tournament. Setting this automatically
    /// goes to that stage.
    /// </summary>
    [ObservableProperty]
    public partial StageViewModel? Next { get; set; }
    partial void OnNextChanged(StageViewModel? value)
    {
        if (value is not null)
        {
            value.Previous = this;
            StrongReferenceMessenger.Default.Send(new NextStageMessage(this, value));
        }
    }

    /// <summary>
    /// The previous stage of the tournament
    /// </summary>
    [ObservableProperty]
    public partial StageViewModel? Previous { get; set; }

    /// <summary>
    /// When a stage represents the final stage of a tournament, this will be true
    /// </summary>
    public virtual bool IsTournamentCompleted => false;

    /// <summary>
    /// Gets the title of the tournament from the previous stage
    /// </summary>
    public virtual string? Event => Previous?.Event;

    /// <summary>
    /// Gets the title of the tournament from the previous stage
    /// </summary>
    public virtual string Title => Previous?.Title ?? string.Empty;

    #endregion

    /// <summary>
    /// Converts into a model
    /// </summary>
    public abstract Stage ToModel();

    /// <summary>
    /// Converts from a model
    /// </summary>
    public static StageViewModel? FromModel(Stage? model) => model switch
    {
        SetupStage ss => SetupStageViewModel.FromModel(ss),
        SquadronsStage sqs => SquadronsStageViewModel.FromModel(sqs),
        PoolsStage ps => PoolsStageViewModel.FromModel(ps),
        SeedingStage sds => SeedingStageViewModel.FromModel(sds),
        BracketStage bs => BracketStageViewModel.FromModel(bs),
        ResultsStage rs => ResultsStageViewModel.FromModel(rs),
        null => null,
        _ => throw new NotSupportedException("Unsupported stage type"),
    };

    /// <summary>
    /// Called when the tournament is saved
    /// </summary>
    public virtual void OnTournamentSaved() => Next?.OnTournamentSaved();

    /// <summary>
    /// Unregisters this stage's message handlers
    /// </summary>
    public void Dispose()
    {
        Next?.Dispose();
        StrongReferenceMessenger.Default.UnregisterAll(this);
        CleanUp();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Allows for cleanup when disposing
    /// </summary>
    protected virtual void CleanUp() { }

    #region Back Navigation

    /// <summary>
    /// Returns to the previous stage
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void GoBack()
    {
        OnGoingBack();
        StrongReferenceMessenger.Default.Send(new PreviousStageMessage(this, Previous));
    }

    /// <summary>
    /// Determines whether navigation to a previous item is possible.
    /// </summary>
    protected virtual bool CanGoBack() => true;

    /// <summary>
    /// Called before going back to the previous stage
    /// </summary>
    protected virtual void OnGoingBack() { }

    #endregion
}
