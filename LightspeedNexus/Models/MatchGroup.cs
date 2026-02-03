using System;

namespace LightspeedNexus.Models;

/// <summary>
/// A group of matches sharing similar settings
/// </summary>
public sealed class MatchGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MatchSettings Settings { get; set; } = new();
    public Guid[] Matches { get; set; } = [];
}