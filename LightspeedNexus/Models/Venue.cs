using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace LightspeedNexus.Models;

/// <summary>
/// A fighter is a Lightspeed competitor who has or will participate in events
/// </summary>
public sealed record Venue(Guid Id,
    string Name,
    string[] Rings) : CollectionObject(Id)
{
    public Venue(Venue other) : base(other)
    {
        Name = other.Name;
        Rings = [.. other.Rings];
    }
}
