using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LightspeedNexus.Messages;
using LightspeedNexus.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LightspeedNexus.ViewModels;

public partial class TournamentViewModel : ViewModelBase
{
    #region Properties

    public Guid Guid { get; protected set; }

    public ObservableCollection<StageViewModel> Stages { get; set; } = [new SetupStageViewModel()];

    #endregion

    #region Commands

    [RelayCommand]
    private static void GoHome() => WeakReferenceMessenger.Default.Send<NavigateHomeMessage>();

    #endregion

    /// <summary>
    /// Creates a brand new tournament
    /// </summary>
    public TournamentViewModel()
    {
        Guid = Guid.NewGuid();
        Stages = [new SetupStageViewModel()];
        Stages[0].PropertyChanged += SettingsChanged;
    }

    /// <summary>
    /// Loads an existing tournament
    /// </summary>
    public TournamentViewModel(Tournament model)
    {
        Guid = model.Id;
        Stages = [.. model.Stages.Select(s => s.ToViewModel())];
        Stages.FirstOrDefault(s => s is SetupStageViewModel)?.PropertyChanged += SettingsChanged;
    }

    /// <summary>
    /// The name of the tournament, e.g., Open Rey
    /// </summary>
    public string Name
    {
        get
        {
            if (Stages.Count > 0 && Stages[0] is SetupStageViewModel Settings)
            {
                StringBuilder sb = new();

                if (Settings.Demographic != Demographic.All)
                {
                    sb.Append(Settings.Demographic.ToString());
                    sb.Append("'s ");
                }

                sb.Append(Settings.SkillLevel.ToString());

                if (Settings.GameMode != GameMode.Standard)
                {
                    sb.Append(' ');
                    sb.Append(Settings.GameMode.ToString());
                }

                if (Settings.ReyAllowed && Settings.RenAllowed && Settings.TanoAllowed)
                    sb.Append(" Mixed Weapons");
                else if (Settings.ReyAllowed && Settings.RenAllowed)
                    sb.Append(" Rey/Ren");
                else if (Settings.ReyAllowed && Settings.TanoAllowed)
                    sb.Append(" Rey/Tano");
                else if (Settings.RenAllowed && Settings.TanoAllowed)
                    sb.Append(" Ren/Tano");
                else if (Settings.ReyAllowed)
                    sb.Append(" Rey");
                else if (Settings.RenAllowed)
                    sb.Append(" Ren");
                else if (Settings.TanoAllowed)
                    sb.Append(" Tano");

                if (!string.IsNullOrEmpty(Settings.SubTitle))
                {
                    sb.Append($" - ");
                    sb.Append(Settings.SubTitle);
                }

                return sb.ToString();
            }
            else
                return "New Tournament";
        }
    }

    /// <summary>
    /// Converts to a <see cref="Tournament"/>
    /// </summary>
    public Tournament ToModel() => new(Guid, [.. Stages.Select(s => s.ToModel())]);

    /// <summary>
    /// Listens for changes in settings to update the name
    /// </summary>
    protected void SettingsChanged(object? sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(Name));
}
