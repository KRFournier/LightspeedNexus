using LiteDB;
using System;
using System.Text.Json.Nodes;

namespace LightspeedNexus.Models;

/// <summary>
/// Represents the class of a weapon used by a fighter in Lightspeed competitions.
/// </summary>
public enum WeaponClass
{
    Rey,
    Ren,
    Tano,
    Dyad
}

/// <summary>
/// The fencer's rating for a specific weapon class.
/// </summary>
public sealed class WeaponRating
{
    public WeaponClass Class { get; set; } = WeaponClass.Rey;
    public Rank Rank { get; set; } = Rank.U;

    public WeaponRating() { }
    public WeaponRating(WeaponClass weaponClass, Rank rank)
    {
        Class = weaponClass;
        Rank = rank;
    }

    /// <summary>
    /// Creates a new instance of the WeaponRating class from a JSON node representing a SaberSport weapon rating.
    /// </summary>
    /// <remarks>If the JSON node does not contain valid or expected data, the method returns null instead of
    /// throwing an exception.</remarks>
    /// <param name="node">A JsonNode containing the weapon rating data to parse. Can be null.</param>
    /// <returns>A WeaponRating instance parsed from the specified JSON node, or null if the node is null or cannot be parsed.</returns>
    public static WeaponRating? FromSaberSport(JsonNode? node)
    {
        if (node is null)
            return null;

        try
        {
            return new()
            {
                Class = Enum.Parse<WeaponClass>(node["type"]?.GetValue<string>() ?? "", true),
                Rank = node["rank"]?.GetValue<string>()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing weapon rating: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// A fighter is a Lightspeed competitor who has or will participate in events.
/// This is a global representation not tied to any specific event or team.
/// Fighters persist between application sessions.
/// </summary>
public class Fighter : CollectionObject
{
    public int? OnlineId { get; set; } = null;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Club { get; set; }
    public Rank Rey { get; set; } = Rank.U;
    public Rank Ren { get; set; } = Rank.U;
    public Rank Tano { get; set; } = Rank.U;

    public Fighter() { }

    /// <summary>
    /// Creates a new Fighter instance from a SaberSport-formatted JSON node.
    /// </summary>
    /// <remarks>The method expects the JSON node to contain required fields such as 'id', 'first_name', and
    /// 'last_name'. If any of these fields are missing or invalid, the method returns null. Optional fields such as
    /// 'club' and 'ranks' are included if present.</remarks>
    /// <param name="node">The JSON node containing fighter data in the SaberSport format. May be null.</param>
    /// <returns>A Fighter instance populated with data from the specified JSON node, or null if the node is null or required
    /// fields are missing or invalid.</returns>
    public static Fighter? FromSaberSport(JsonNode? node)
    {
        if (node is null)
            return null;

        try
        {
            Fighter fighter = new();

            // these can throw because they are required
            fighter.OnlineId = node["id"]?.GetValue<int>();
            fighter.FirstName = node["first_name"]?.GetValue<string>() ?? string.Empty;
            fighter.LastName = node["last_name"]?.GetValue<string>() ?? string.Empty;

            // optional
            if (node["club"] is JsonValue jsonClub && jsonClub.GetValueKind() == System.Text.Json.JsonValueKind.String)
                fighter.Club = jsonClub.GetValue<string>();

            if (node["ranks"] is JsonArray ranks)
            {
                foreach (var rankNode in ranks)
                {
                    var rating = WeaponRating.FromSaberSport(rankNode);
                    if (rating is not null)
                    {
                        switch (rating.Class)
                        {
                            case WeaponClass.Rey when rating.Rank > fighter.Rey:
                                fighter.Rey = rating.Rank;
                                break;
                            case WeaponClass.Ren when rating.Rank > fighter.Ren:
                                fighter.Ren = rating.Rank;
                                break;
                            case WeaponClass.Tano when rating.Rank > fighter.Tano:
                                fighter.Tano = rating.Rank;
                                break;
                            default:
                                Console.WriteLine($"Unknown weapon class: {rating.Class}");
                                break;
                        }
                    }
                }
            }

            // one last check to make sure there's a name
            if (string.IsNullOrEmpty(fighter.FirstName) || string.IsNullOrEmpty(fighter.LastName))
                return null;

            return fighter;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing fighter: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Represents the fighter as a string in "FirstName LastName" format.
    /// </summary>
    public override string ToString() => Name;

    /// <summary>
    /// Gets the full name, consisting of the first and last name combined.
    /// </summary>
    [BsonIgnore]
    public string Name => $"{FirstName} {LastName}";

    /// <summary>
    /// Convenient method to convert this Fighter into a Registree.
    /// </summary>
    public Registree ToRegistree(bool usesEffectiveRank = false, WeaponClass weaponOfChoice = WeaponClass.Rey) => new()
    {
        Id = Id,
        OnlineId = OnlineId,
        FirstName = FirstName,
        LastName = LastName,
        Club = Club,
        Rey = Rey,
        Ren = Ren,
        Tano = Tano,
        UsesEffectiveRank = usesEffectiveRank,
        WeaponOfChoice = weaponOfChoice
    };
}
