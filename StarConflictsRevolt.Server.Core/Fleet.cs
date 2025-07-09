using System.Collections.Generic;

namespace StarConflictsRevolt.Server.Core;

public enum FleetStatus { Idle, EnRoute, Blockading, InCombat }

public record Fleet(
    Guid Id,
    string Name,
    List<Ship> Ships,
    FleetStatus Status,
    Guid? OrbitingPlanetId
) : GameObject
{
    public Fleet() : this(Guid.Empty, string.Empty, new List<Ship>(), FleetStatus.Idle, null) { }
} 