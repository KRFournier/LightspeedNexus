using LightspeedNexus.Models;
using LiteDB;

namespace LightspeedNexus.Services;

public class StorageService
{
    public StorageService()
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
    private static LiteDatabase GetDatabase() =>
#if DEBUG
        new(Path.Combine(Dir, "debug_data.db"));
#else
        new LiteDatabase(Path.Combine(Dir, "data.db"));
#endif


    /// <summary>
    /// Builds the collection name from the type name
    /// </summary>
    private static string GetCollectionName<T>() => typeof(T).Name.ToLowerInvariant() + "s";

    #region Settings

    ///// <summary>
    ///// Writes a document to the database in the specified collection
    ///// </summary>
    //public  void WriteSettings(BsonDocument document)
    //{
    //    using var db = GetDatabase();
    //    var collection = db.GetCollection("settings");
    //    collection.Upsert(document);
    //}

    ///// <summary>
    ///// Gets the document with the given id
    ///// </summary>
    //public  BsonDocument? ReadSettings(BsonValue id)
    //{
    //    using var db = GetDatabase();
    //    var collection = db.GetCollection("settings");
    //    return collection.FindById(id);
    //}

    /// <summary>
    /// Writes a document to the database in the specified collection
    /// </summary>
    public void WriteRings(IEnumerable<string> rings)
    {
        string[] ringArray = [.. rings];
        if (ringArray.Length > 0 && !string.IsNullOrEmpty(ringArray[0]))
        {
            using var db = GetDatabase();
            var collection = db.GetCollection("settings");

            var doc = new BsonDocument
            {
                ["_id"] = 1,
                ["rings"] = new BsonArray(ringArray.Select(r => new BsonValue(r)))
            };

            collection.Upsert(doc);
        }
    }

    /// <summary>
    /// Gets the document with the given id
    /// </summary>
    public string[]? ReadRings()
    {
        using var db = GetDatabase();
        var collection = db.GetCollection("settings");
        return collection.FindById(new BsonValue(1))?["rings"]?.AsArray.Select(b => b.AsString).ToArray();
    }

    #endregion

    #region Tournaments

    /// <summary>
    /// Writes a tournament item, ensuring proper indexes
    /// </summary>
    public void WriteTournament(Tournament item)
    {
        using var db = GetDatabase();
        var collection = db.GetCollection<Tournament>(GetCollectionName<Tournament>());
        collection.Upsert(item);
        collection.EnsureIndex("SetupStage.Date");
        collection.EnsureIndex("IsCompleted");
    }

    public Tournament? GetTournament(Guid guid)
    {
        using var db = GetDatabase();
        var collection = db.GetCollection<Tournament>(GetCollectionName<Tournament>());
        return collection.FindOne(t => t.Id == guid);
    }

    public Tournament[] ReadAllTournaments()
    {
        using var db = GetDatabase();
        var collection = db.GetCollection<Tournament>(GetCollectionName<Tournament>());
        return [.. collection.FindAll()];
    }

    public Tournament[] ReadRecentTournaments()
    {
        using var db = GetDatabase();
        var collection = db.GetCollection<Tournament>(GetCollectionName<Tournament>());
        return collection.Query()
            .Where(t => !t.IsCompleted)
            .OrderByDescending(t => t.SetupStage.Date)
            .ToArray();
    }

    public int CountTournaments()
    {
        using var db = GetDatabase();
        var collection = db.GetCollection<Tournament>(GetCollectionName<Tournament>());
        return collection.Query().Count();
    }

    #endregion

    #region Matches

    /// <summary>
    /// Writes a bunch of matches
    /// </summary>
    /// <param name="matches"></param>
    public void WriteMatches(IEnumerable<Match?> matches)
    {
        using var db = GetDatabase();
        var collection = db.GetCollection<Match>(GetCollectionName<Match>());
        collection.Upsert(matches.Where(m => m is not null).Select(m => m!));
    }

    /// <summary>
    /// Writes a match item, ensuring proper indexes
    /// </summary>
    public void WriteMatch(Match? match)
    {
        if (match is not null)
        {
            using var db = GetDatabase();
            var collection = db.GetCollection<Match>(GetCollectionName<Match>());
            collection.Upsert(match);
        }
    }

    /// <summary>
    /// Gets the match with the given id
    /// </summary>
    public Match? GetMatch(Guid id)
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<Match>(GetCollectionName<Match>());
        return fightersCollection.FindById(id);
    }

    #endregion

    /// <summary>
    /// Counts a type of collection
    /// </summary>
    public int Count<T>() where T : CollectionObject
    {
        using var db = GetDatabase();
        var collection = db.GetCollection<T>(GetCollectionName<T>());
        return collection.Query().Count();
    }

    /// <summary>
    /// Saves an item
    /// </summary>
    public void Write<T>(T item) where T : CollectionObject
    {
        using var db = GetDatabase();
        var collection = db.GetCollection<T>(GetCollectionName<T>());
        collection.Upsert(item);
    }

    /// <summary>
    /// Gets all items in the collection
    /// </summary>
    public T[] ReadAll<T>() where T : CollectionObject
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<T>(GetCollectionName<T>());
        return [.. fightersCollection.FindAll()];
    }

    /// <summary>
    /// Gets the item with the given id
    /// </summary>
    public T Get<T>(Guid id) where T : CollectionObject
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<T>(GetCollectionName<T>());
        return fightersCollection.FindById(id);
    }

    /// <summary>
    /// Deletes the given item
    /// </summary>
    public void Delete<T>(T item) where T : CollectionObject
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<T>(GetCollectionName<T>());
        fightersCollection.Delete(item.Id);
    }

    /// <summary>
    /// Deletes the given item
    /// </summary>
    public void Delete<T>(Guid id) where T : CollectionObject
    {
        using var db = GetDatabase();
        var fightersCollection = db.GetCollection<T>(GetCollectionName<T>());
        fightersCollection.Delete(id);
    }
}
