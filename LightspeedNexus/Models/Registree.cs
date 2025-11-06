using LightspeedNexus.ViewModels;
using System;

namespace LightspeedNexus.Models;

/// <summary>
/// Represents a fighter that is registered to participate in a tournament.
/// Duplicates Fighter info to allow for historical record-keeping even if the Fighter data changes later.
/// </summary>
public sealed class Registree : CollectionObject
{
    public int? OnlineId { get; set; } = null;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Club { get; set; }
    public Rank Rey { get; set; } = Rank.U;
    public Rank Ren { get; set; } = Rank.U;
    public Rank Tano { get; set; } = Rank.U;

    public bool UsesEffectiveRank { get; set; } = false;
    public WeaponClass WeaponOfChoice { get; set; } = WeaponClass.Rey;

    public Registree() { }
    public Registree(Guid id, int? onlineId, string firstName, string lastName, string? club,
        Rank rey, Rank ren, Rank tano, bool usesEffectiveRank, WeaponClass weaponOfChoice)
        : base(id)
    {
        OnlineId = onlineId;
        FirstName = firstName;
        LastName = lastName;
        Club = club;
        Rey = rey;
        Ren = ren;
        Tano = tano;
        UsesEffectiveRank = usesEffectiveRank;
        WeaponOfChoice = weaponOfChoice;
    }
    public Registree(Fighter fighter, bool usesEffectiveRank = false, WeaponClass weaponOfChoice = WeaponClass.Rey)
        : base(fighter.Id)
    {
        OnlineId = fighter.OnlineId;
        FirstName = fighter.FirstName;
        LastName = fighter.LastName;
        Club = fighter.Club;
        Rey = fighter.Rey;
        Ren = fighter.Ren;
        Tano = fighter.Tano;
        UsesEffectiveRank = usesEffectiveRank;
        WeaponOfChoice = weaponOfChoice;
    }

    public Fighter ToFighter() => new(Id, OnlineId, FirstName, LastName, Club, Rey, Ren, Tano);
    public new RegistreeViewModel ToViewModel() => new(this);
}
