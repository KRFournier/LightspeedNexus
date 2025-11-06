using System;

namespace LightspeedNexus.Models;

/// <summary>
/// Base class for objects stored in collections.
/// </summary>
public class CollectionObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public CollectionObject() { }

    public CollectionObject(Guid id)
    {
        Id = id;
    }
}
