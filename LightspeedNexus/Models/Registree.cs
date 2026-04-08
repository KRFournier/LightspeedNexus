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

    public Fighter ToFighter() => new()
    {
        Id = Id,
        OnlineId = OnlineId,
        FirstName = FirstName,
        LastName = LastName,
        Club = Club,
        Rey = Rey,
        Ren = Ren,
        Tano = Tano
    };
}
