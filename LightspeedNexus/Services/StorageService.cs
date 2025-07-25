using LightspeedNexus.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LightspeedNexus.Services;

public static class StorageService
{
    public static void RegisterSerializers()
    {
        // DateOnly
        BsonMapper.Global.RegisterType<DateOnly>(
            serialize: date => date.ToString(),
            deserialize: str => DateOnly.TryParse(str, out DateOnly dateOnly) ? dateOnly : DateOnly.MinValue
            );

        // Ranks
        BsonMapper.Global.RegisterType<Rank>(
            serialize: rank => rank.ToString(),
            deserialize: str => new Rank(str)
            );
    }

    /// <summary>
    /// The storage location
    /// </summary>
    private static string Dir
    {
        get
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lightspeed\\Nexus");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return dir;
        }
    }

    /// <summary>
    /// The database
    /// </summary>
    private static LiteDatabase GetDatabase()
    {
#if DEBUG
        return new LiteDatabase(Path.Combine(Dir, "debug_data.db"));
#else
        return new LiteDatabase(Path.Combine(Dir, "data.db"));
#endif
    }

    /// <summary>
    /// Writes a document to the database in the specified collection
    /// </summary>
    public static void WriteDocument(BsonDocument document, string collectionName)
    {
        using var db = GetDatabase();
        var collection = db.GetCollection(collectionName);
        collection.Upsert(document);
    }

    /// <summary>
    /// Gets the document with the given id
    /// </summary>
    public static BsonDocument? ReadDocument(BsonValue id, string collectionName)
    {
        using var db = GetDatabase();
        var collection = db.GetCollection(collectionName);
        return collection.FindById(id);
    }

    /// <summary>
    /// Saves a fighter
    /// </summary>
    public static void WriteFighter(Fighter fighter)
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<Fighter>("fighters");
        fightersCollection.Upsert(fighter);
    }

    /// <summary>
    /// Gets all fighters in the database
    /// </summary>
    public static Fighter[] ReadAllFighters()
    {

        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<Fighter>("fighters");
        return [.. fightersCollection.FindAll()];
    }

    /// <summary>
    /// Deletes the given fighter
    /// </summary>
    public static void DeleteFighter(Fighter fighter)
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<Fighter>("fighters");
        fightersCollection.Delete(fighter.Id);
    }
}
