using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LightspeedNexus.Models;

/// <summary>
/// The fencer's rating for a specific weapon class.
/// </summary>
public class WeaponRating
{
    public WeaponClass Class { get; set; } = WeaponClass.Rey;
    public Rank Rank { get; set; } = Rank.U;
    public DateOnly Earned { get; set; } = DateOnly.MinValue;

    public static WeaponRating? FromSaberSport(JsonNode? node)
    {
        if (node is null)
            return null;

        WeaponRating rating = new();

        try
        {
            // required
            rating.Class = Enum.Parse<WeaponClass>(node["type"]?.GetValue<string>() ?? "", true);
            rating.Rank = node["rank"]?.GetValue<string>();

            // optional
            if (node["earned"] is JsonValue earned && earned.GetValueKind() == System.Text.Json.JsonValueKind.String)
            {
                var earnedStr = earned.GetValue<string>();
                if (!DateOnly.TryParse(earnedStr, out DateOnly date))
                {
                    if (DateOnly.TryParseExact(earnedStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                        rating.Earned = date;
                }
                else
                    rating.Earned = date;
            }

            return rating;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing weapon rating: {ex.Message}");
            return null;
        }
    }

    public void UpdateFrom(WeaponRating other)
    {
        if (other.Rank > Rank)
        {
            Rank = other.Rank;
            if (other.Earned > DateOnly.MinValue)
                Earned = other.Earned;
        }
    }
}

/// <summary>
/// A fighter is a Lightspeed competitor who has or will participate in events
/// </summary>
public class Fighter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int? OnlineId { get; set; } = null;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Club { get; set; } = null;
    public WeaponRating[] Ratings { get; set; } = [
        new() { Class = WeaponClass.Rey, Rank = Rank.U, Earned = DateOnly.MinValue },
        new() { Class = WeaponClass.Ren, Rank = Rank.U, Earned = DateOnly.MinValue },
        new() { Class = WeaponClass.Tano, Rank = Rank.U, Earned = DateOnly.MinValue },
        ];

    public static Fighter? FromSaberSport(JsonNode? node)
    {
        if (node is null)
            return null;

        Fighter fighter = new();

        try
        {
            // these can throw because they are required
            fighter.OnlineId = node["id"]?.GetValue<int>();
            fighter.FirstName = node["first_name"]?.GetValue<string>() ?? string.Empty;
            fighter.LastName = node["last_name"]?.GetValue<string>() ?? string.Empty;

            // optional
            if (node["club"] is JsonValue club && club.GetValueKind() == System.Text.Json.JsonValueKind.String)
                fighter.Club = club.GetValue<string>();
            if (node["ranks"] is JsonArray ranks)
                fighter.Ratings = [.. ranks.Select(n => WeaponRating.FromSaberSport(n)).Where(r => r is not null)!];

            // one last check to make sure there's a name
            if(string.IsNullOrEmpty(fighter.FirstName) || string.IsNullOrEmpty(fighter.LastName))
                return null;

            return fighter;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error parsing fighter: {ex.Message}");
            return null;
        }
    }

    public void UpdateFrom(Fighter other)
    {
        if (string.IsNullOrEmpty(Club))
            Club = other.Club;

        foreach(var otherRating in other.Ratings)
            Ratings.FirstOrDefault(r => r.Class == otherRating.Class)?.UpdateFrom(otherRating);
    }
}
