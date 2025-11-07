using CommunityToolkit.Mvvm.ComponentModel;
using LightspeedNexus.Models;
using System;

namespace LightspeedNexus.ViewModels;

/// <summary>
/// A tournament stage. Base class for all stages
/// </summary>
public abstract partial class StageViewModel : ViewModelBase    
{
    /// <summary>
    /// Converts into a model
    /// </summary>
    public abstract Stage ToModel();
}
