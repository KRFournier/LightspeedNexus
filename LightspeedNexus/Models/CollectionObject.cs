using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedNexus.Models;

/// <summary>
/// Base class for objects stored in collections.
/// </summary>
public class CollectionObject
{
    public Guid Id { get; set; } = Guid.Empty;

    public CollectionObject() { }

    public CollectionObject(CollectionObject other)
    {
        Id = other.Id;
    }
}
