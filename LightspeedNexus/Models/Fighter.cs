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

    public static WeaponRating? FromSaberSport(JsonNode? node)
    {
        if (node is null)
            return null;

        WeaponRating rating = new();

        try
        {
            rating.Class = Enum.Parse<WeaponClass>(node["type"]?.GetValue<string>() ?? "", true);
            rating.Rank = node["rank"]?.GetValue<string>();
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
            Rank = other.Rank;
    }
}

/// <summary>
/// A fighter is a Lightspeed competitor who has or will participate in events
/// </summary>
public class Fighter : CollectionObject
{
    public int? OnlineId { get; set; } = null;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Club { get; set; } = null;
    public WeaponRating Rey { get; set; } = new() { Class = WeaponClass.Rey, Rank = Rank.U };
    public WeaponRating Ren { get; set; } = new() { Class = WeaponClass.Ren, Rank = Rank.U };
    public WeaponRating Tano { get; set; } = new() { Class = WeaponClass.Tano, Rank = Rank.U };

    public Fighter() : base() { }

    public Fighter(Fighter other) : base(other)
    {
        OnlineId = other.OnlineId;
        FirstName = other.FirstName;
        LastName = other.LastName;
        Club = other.Club;
        Rey = new WeaponRating { Class = other.Rey.Class, Rank = other.Rey.Rank };
        Ren = new WeaponRating { Class = other.Ren.Class, Rank = other.Ren.Rank };
        Tano = new WeaponRating { Class = other.Tano.Class, Rank = other.Tano.Rank };
    }

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
            {
                foreach (var rankNode in ranks)
                {
                    var rating = WeaponRating.FromSaberSport(rankNode);
                    if (rating is not null)
                    {
                        switch (rating.Class)
                        {
                            case WeaponClass.Rey:
                                fighter.Rey.UpdateFrom(rating);
                                break;
                            case WeaponClass.Ren:
                                fighter.Ren.UpdateFrom(rating);
                                break;
                            case WeaponClass.Tano:
                                fighter.Tano.UpdateFrom(rating);
                                break;
                            default:
                                Console.WriteLine($"Unknown weapon class: {rating.Class}");
                                break;
                        }
                    }
                }
            }

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
        Rey.UpdateFrom(other.Rey);
        Ren.UpdateFrom(other.Ren);
        Tano.UpdateFrom(other.Tano);
    }

    public override string ToString() => FullName;

    public string FullName => $"{FirstName} {LastName}";
}
