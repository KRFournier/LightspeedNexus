using System;

namespace LightspeedNexus.Models;

/// <summary>
/// The type of event
/// </summary>
public enum EventType
{
    None,
    Admin,
    CheckIn,
    Pool,
    Bracket,
    DoubleElim
}

/// <summary>
/// An event, used just for scheduling. Represents a logical unit of
/// a competition
/// </summary>
public sealed record Event(Guid Id,
    EventType Type,
    string Name,
    int Day,
    TimeOnly Start,
    TimeSpan Duration) : CollectionObject(Id)
{
    public Event(Event other) : base(other)
    {
        Type = other.Type;
        Name = other.Name;
        Start = other.Start;
    }
}
