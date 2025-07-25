using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedNexus.Models;

/// <summary>
/// A competition, which is composed of multiple events and can have multiple participants.
/// </summary>
public class Competition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public DateOnly Start { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public DateOnly End { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}
