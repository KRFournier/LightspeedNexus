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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// The tournament settings
/// </summary>
public partial class SquadronsStageViewModel : StageViewModel
{
    #region Properties

    [ObservableProperty]
    public partial string Title { get; set; }

    public ObservableCollection<ParticipantViewModel> Participants { get; set; } = [];

    [ObservableProperty]
    public partial bool IsAutoAssigned { get; set; }

    #endregion

    /// <summary>
    /// Creates brand new settings
    /// </summary>
    public SquadronsStageViewModel(string title) : base("Squadrons")
    {
        Title = title;
        IsAutoAssigned = true;
    }

    /// <summary>
    /// Loads settings from a model
    /// </summary>
    public SquadronsStageViewModel(string title, SquadronsStage model) : this(title)
    {
        //Participants = [..model.Participants.Select(p => new ParticipantViewModel(p))];
        IsAutoAssigned = model.IsAutoAssigned;
    }

    /// <summary>
    /// Converts into a model
    /// </summary>
    public override SquadronsStage ToModel() => new();

    [RelayCommand]
    private void StartPools()
    {
        //WeakReferenceMessenger.Default.Send(new NextStageMessage(StageType.Registration));
    }
}
