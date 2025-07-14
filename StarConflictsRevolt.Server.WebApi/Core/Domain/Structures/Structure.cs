using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;
using Planet = StarConflictsRevolt.Server.WebApi.Core.Domain.Planets.Planet;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Structures;

public record Structure(
    StructureVariant Variant,
    Planet Planet,
    Guid OwnerId,
    int Health = 100,
    int MaxHealth = 100,
    bool IsOperational = true,
    DateTime? LastProductionTime = null
) : GameObject;