using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A tournament stage. Base class for all stages
/// </summary>
public abstract partial class StageViewModel(string name) : ViewModelBase
{
    #region Properties

    [ObservableProperty]
    public partial string Name { get; set; } = name;

    [ObservableProperty]
    public partial StageViewModel? Next { get; set; }
    partial void OnNextChanged(StageViewModel? value) => WeakReferenceMessenger.Default.Send(new StageChangedMessage(this));

    /// <summary>
    /// When a stage represents the final stage of a tournament, this will be true
    /// </summary>
    public virtual bool IsTournamentCompleted => false;

    #endregion

    /// <summary>
    /// Converts into a model
    /// </summary>
    public abstract Stage ToModel();
}
