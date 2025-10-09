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
    public static void WriteSettings(BsonDocument document)
    {
        using var db = GetDatabase();
        var collection = db.GetCollection("settings");
        collection.Upsert(document);
    }

    /// <summary>
    /// Gets the document with the given id
    /// </summary>
    public static BsonDocument? ReadSettings(BsonValue id)
    {
        using var db = GetDatabase();
        var collection = db.GetCollection("settings");
        return collection.FindById(id);
    }

    /// <summary>
    /// Builds the collection name from the type name
    /// </summary>
    private static string GetCollectionName<T>() => typeof(T).Name.ToLowerInvariant() + "s";

    /// <summary>
    /// Saves an item
    /// </summary>
    public static void Write<T>(T item) where T : CollectionObject
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<T>(GetCollectionName<T>());
        fightersCollection.Upsert(item);
    }

    /// <summary>
    /// Gets all items in the collection
    /// </summary>
    public static T[] ReadAll<T>() where T : CollectionObject
    {

        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<T>(GetCollectionName<T>());
        return [.. fightersCollection.FindAll()];
    }

    /// <summary>
    /// Gets the item with the given id
    /// </summary>
    public static T Get<T>(Guid id) where T : CollectionObject
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<T>(GetCollectionName<T>());
        return fightersCollection.FindById(id);
    }

    /// <summary>
    /// Deletes the given item
    /// </summary>
    public static void Delete<T>(T item) where T : CollectionObject
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<T>(GetCollectionName<T>());
        fightersCollection.Delete(item.Id);
    }
}
