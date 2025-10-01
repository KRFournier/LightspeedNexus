using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedNexus.Models;

/// <summary>
/// Estimator for event durations and scheduling.
/// </summary>
public static class EventEstimator
{
    //public static TimeSpan Estimate(Event @event, Competition competition) => @event.Type switch
    //{
    //    EventType.Admin => TimeSpan.FromMinutes(30),
    //    EventType.CheckIn => EstimateCheckIn(@event, competition.RingCount),
    //    EventType.Pool => TimeSpan.FromHours(2),
    //    EventType.Bracket => TimeSpan.FromHours(3),
    //    EventType.DoubleElim => TimeSpan.FromHours(4),
    //    _ => TimeSpan.FromHours(1)
    //};

    //public static TimeSpan EstimateCheckIn(Event @event, int ) => TimeSpan.FromMinutes(15);
}
