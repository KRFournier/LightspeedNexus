using LightspeedNexus.Models;

namespace LightspeedNexus;

public static class Extensions
{
    /// <summary>
    /// Convenient method to convert this Fighter into a Registree.
    /// </summary>
    public static Registree ToRegistree(this Fighter fighter, bool usesEffectiveRank = false, WeaponClass weaponOfChoice = WeaponClass.Rey) => new()
    {
        Id = fighter.Id,
        OnlineId = fighter.OnlineId,
        FirstName = fighter.FirstName,
        LastName = fighter.LastName,
        Club = fighter.Club,
        Rey = fighter.Rey,
        Ren = fighter.Ren,
        Tano = fighter.Tano,
        UsesEffectiveRank = usesEffectiveRank,
        WeaponOfChoice = weaponOfChoice
    };
}
