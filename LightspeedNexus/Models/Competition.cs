using System;
using System.Collections.Generic;
using LiteDB;

namespace LightspeedNexus.Models;

/// <summary>
/// A competition, which is composed of multiple events and can have multiple participants.
/// </summary>
public class Competition : CollectionObject
{
    public string Name { get; set; } = "";
    public DateOnly Start { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public int Days { get; set; } = 1;

    public List<Fighter> Roster { get; set; } = [];

    public List<Event> Events { get; set; } = [];

    [BsonRef("venues")]
    public Venue? Venue { get; set; } = null;

    public int RingCount => Venue?.Rings.Count ?? 1;

    public Competition() : base() { }

    public Competition(Competition other) : base(other)
    {
        Name = other.Name;
        Start = other.Start;
        Days = other.Days;
    }
}
