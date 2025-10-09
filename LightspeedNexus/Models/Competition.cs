using System;
using System.Collections.Generic;
using LiteDB;

namespace LightspeedNexus.Models;

/// <summary>
/// A competition, which is composed of multiple events and can have multiple participants.
/// </summary>
public record Competition(Guid Id,
    DateOnly Start,
    int Days,
    Fighter[] Roster,
    Event[] Event,
    [property: BsonRef("venues")] Venue? Venue,
    int RingCount) : CollectionObject(Id)
{
    public Competition(Competition other) : base(other)
    {
        Start = other.Start;
        Days = other.Days;
        Roster = other.Roster;
        Event = other.Event;
        Venue = other.Venue;
        Start = other.Start;
        Days = other.Days;
    }
}
