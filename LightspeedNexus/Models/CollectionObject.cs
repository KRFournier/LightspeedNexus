using System;

namespace LightspeedNexus.Models;

/// <summary>
/// Base class for objects stored in collections.
/// </summary>
public record CollectionObject(Guid Id)
{
    public CollectionObject() : this(Guid.NewGuid()) { }
}
