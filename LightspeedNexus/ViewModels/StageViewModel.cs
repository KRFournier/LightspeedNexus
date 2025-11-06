using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A tournament stage. Base class for all stages
/// </summary>
public abstract partial class StageViewModel : ViewModelBase    
{
    #region Properties

    [ObservableProperty]
    public partial string Name { get; set; }

    #endregion

    /// <summary>
    /// Creates a new stage with a name derived from the class name
    /// </summary>
    public StageViewModel()
    {
        Name = GetType().Name.Replace("StageViewModel", "");
    }


    /// <summary>
    /// Creates a new stage with the given name
    /// </summary>
    public StageViewModel(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Loads stage from a model
    /// </summary>
    public StageViewModel(Stage model)
    {
        Name = model.Name;
    }

    /// <summary>
    /// Converts into a model
    /// </summary>
    public abstract Stage ToModel();
}
