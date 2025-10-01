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
public class Event : CollectionObject
{
    /// <summary>
    /// The type of event
    /// </summary>
    public EventType Type { get; set; } = EventType.None;

    /// <summary>
    /// The name of the event
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Day 0 is the first day of the competition. We don't use fixed dates
    /// to make it easier to reschedule competitions.
    /// </summary>
    public int Day { get; set; } = 0;

    /// <summary>
    /// The time the event starts on the day it is scheduled for.
    /// </summary>
    public TimeOnly Start { get; set; } = TimeOnly.MinValue;

    /// <summary>
    /// The duration of the event. Default is 1 hour.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);

    public Event() : base() { }

    public Event(Event other) : base(other)
    {
        Type = other.Type;
        Name = other.Name;
        Start = other.Start;
    }
}
