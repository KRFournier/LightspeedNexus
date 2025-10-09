using LightspeedNexus.ViewModels;
using LiteDB;
using System;
using System.Reflection;
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
public sealed record WeaponRating(WeaponClass Class, Rank Rank)
{
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
            return new(
                Enum.Parse<WeaponClass>(node["type"]?.GetValue<string>() ?? "", true),
                node["rank"]?.GetValue<string>()
                );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing weapon rating: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// A fighter is a Lightspeed competitor who has or will participate in events
/// </summary>
public record Fighter(Guid Id,
    int? OnlineId, string FirstName, string LastName, string? Club,
    Rank Rey, Rank Ren, Rank Tano
    ) : CollectionObject(Id)
{
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
            // these can throw because they are required
            var onlineId = node["id"]?.GetValue<int>();
            var firstName = node["first_name"]?.GetValue<string>() ?? string.Empty;
            var lastName = node["last_name"]?.GetValue<string>() ?? string.Empty;
            string? club = null;
            Rank rey = Rank.U;
            Rank ren = Rank.U;
            Rank tano = Rank.U;

            // optional
            if (node["club"] is JsonValue jsonClub && jsonClub.GetValueKind() == System.Text.Json.JsonValueKind.String)
                club = jsonClub.GetValue<string>();

            if (node["ranks"] is JsonArray ranks)
            {
                foreach (var rankNode in ranks)
                {
                    var rating = WeaponRating.FromSaberSport(rankNode);
                    if (rating is not null)
                    {
                        switch (rating.Class)
                        {
                            case WeaponClass.Rey when rating.Rank > rey:
                                rey = rating.Rank;
                                break;
                            case WeaponClass.Ren when rating.Rank > ren:
                                ren = rating.Rank;
                                break;
                            case WeaponClass.Tano when rating.Rank > tano:
                                tano = rating.Rank;
                                break;
                            default:
                                Console.WriteLine($"Unknown weapon class: {rating.Class}");
                                break;
                        }
                    }
                }
            }

            // one last check to make sure there's a name
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return null;

            return new(Guid.NewGuid(), onlineId, firstName, lastName, club, rey, ren, tano);
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
    public override string ToString() => FullName;

    /// <summary>
    /// Gets the full name, consisting of the first and last name combined.
    /// </summary>
    [BsonIgnore]
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Converts the current fighter entity to a corresponding view model representation.
    /// </summary>
    /// <returns>A <see cref="FighterViewModel"/> instance containing the fighter's data mapped for presentation.</returns>
    public FighterViewModel ToViewModel() => new(this);
}
