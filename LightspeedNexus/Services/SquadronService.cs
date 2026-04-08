using Avalonia.Controls;
using Avalonia.Media;
using Lightspeed.ViewModels;
using LightspeedNexus.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LightspeedNexus.Services;

public class SquadronService(IServiceProvider serviceProvider)
{
    /// <summary>
    /// Some names and colors we can use for squadrons
    /// </summary>
    private readonly (string Name, string Color)[] _squadronDefinitions = [
        ("Red", "Red"),
        ("Blue", "Blue"),
        ("Green", "Green"),
        ("Purple", "Purple"),
        ("Light", "White"),
        ("Dark", "Gray"),
        ("Star", "Yellow"),
        ("Sun", "Orange"),
        ("Laser", "Lime"),
        ("Ice", "Teal"),
        ("Nova", "Violet"),
        ("Imperial", "Magenta"),
        ];

    /// <summary>
    /// The maximum number of squadrons supported
    /// </summary>
    public int MaxSquadrons => _squadronDefinitions.Length;

    /// <summary>
    /// The maximum size an automatic squadron can be
    /// </summary>
    public const int AutoSquadronSize = 7;

    /// <summary>
    /// Finds color of a squadron with the given name. If the name is not found, returns "Transparent".
    /// </summary>
    public string FindSquadronColor(string name) => _squadronDefinitions.FirstOrDefault(t => t.Item1 == name, ("Unknown", "Transparent")).Item2;

    /// <summary>
    /// Gets the name and color of a squadron by its index.
    /// </summary>
    public (string Name, string Color) GetSquadronDefinition(int index) => index >= 0 && index < _squadronDefinitions.Length ? _squadronDefinitions[index] : ("Unknown", "Transparent");

    /// <summary>
    /// Creates a new squadron using the given index into SquadronNames
    /// </summary>
    public SquadronViewModel GenerateSquadron(int newSquadronIndex)
    {
        if (newSquadronIndex > _squadronDefinitions.Length)
            throw new ArgumentOutOfRangeException(nameof(newSquadronIndex), $"Cannot create squadron with index {newSquadronIndex} because it exceeds the number of available squadron names ({_squadronDefinitions.Length}).");

        var squadron = serviceProvider.GetRequiredService<SquadronViewModel>();
        squadron.Name = _squadronDefinitions[newSquadronIndex].Name;
        squadron.Color = App.Current?.FindResource($"{_squadronDefinitions[newSquadronIndex].Color}Brush") as IBrush ?? Brushes.Transparent;
        return squadron;
    }

    /// <summary>
    /// Uses a bin packing algorithm to repopulate the squadrons in a balanced fashion
    /// </summary>
    public void RebalanceSquadrons(SquadronsStageViewModel squadronsStage)
    {
        // auto calculate the squadrons, or base it on the requested number
        int new_count = squadronsStage.IsAutoAssigned
            ? (int)Math.Ceiling(squadronsStage.Participants.Count / (double)AutoSquadronSize)
            : Math.Max(squadronsStage.Squadrons.Count, squadronsStage.MinSquadrons);

        // remove extra squadrons from the end
        while (squadronsStage.Squadrons.Count > new_count)
            squadronsStage.Squadrons.RemoveAt(squadronsStage.Squadrons.Count - 1);

        // we shouldn't have more squadrons than we have names to give them
        if (new_count > _squadronDefinitions.Length)
            throw new InvalidOperationException($"Cannot have more than {_squadronDefinitions.Length} squadrons.");

        if (new_count > 0)
        {
            // add or remove squadrons
            for (int i = squadronsStage.Squadrons.Count; i < new_count; i++)
                squadronsStage.Squadrons.Add(GenerateSquadron(squadronsStage.Squadrons.Count));

            // reset members
            foreach (var s in squadronsStage.Squadrons)
                s.Clear();

            // group participants by power level and assign from highest to lowest
            foreach (var group in squadronsStage.Participants.GroupBy(p => p.PowerLevel).OrderByDescending(g => g.Key))
                RandomlyAssign(squadronsStage, [.. group]);

            // move last participant from squadrons that are 2+ larger than the smallest squadrons
            while (squadronsStage.Squadrons.Max(s => s.Participants.Count) - squadronsStage.Squadrons.Min(s => s.Participants.Count) > 1)
            {
                var small = squadronsStage.Squadrons.MinBy(s => s.Participants.Count);
                if (small is not null)
                {
                    var big = squadronsStage.Squadrons.MaxBy(s => s.Participants.Count);
                    if (big is not null && big.Participants.Count > 0)
                    {
                        int w = big.Participants[^1].PowerLevel;
                        small.Participants.Add(big.Participants[^1]);
                        small.Weight += w;
                        big.Weight -= w;
                        big.Participants.RemoveAt(big.Participants.Count - 1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Assigns given players to the smallest squadron. If multiple squadrons
    /// tie for the smallest, then the player is assigned randomly.
    /// </summary>
    private static void RandomlyAssign(SquadronsStageViewModel squadronsStage, IList<ParticipantViewModel> participants)
    {
        Random r = new();

        // place each player into the squadron with the smallest total Value
        while (participants.Count > 0)
        {
            int i = r.Next(participants.Count);
            var participant = participants[i];
            int w = participant.PowerLevel;
            var squadron = squadronsStage.Squadrons.MinBy(s => s.Weight);
            if (squadron is not null)
            {
                squadron.Participants.Add(participant);
                squadron.Weight += w;
            }
            participants.RemoveAt(i);
        }
    }
}
