using StarConflictsRevolt.Server.Domain.Enums;
using StarConflictsRevolt.Server.Domain.Gameplay;
using Planet = StarConflictsRevolt.Server.Domain.Planets.Planet;

namespace StarConflictsRevolt.Server.Domain.Structures;

public record Structure(
    StructureVariant Variant,
    Planet Planet,
    Guid OwnerId,
    int Health = 100,
    int MaxHealth = 100,
    bool IsOperational = true,
    DateTime? LastProductionTime = null
) : GameObject;