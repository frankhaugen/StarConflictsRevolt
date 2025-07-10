using StarConflictsRevolt.Server.WebApi.Enums;

namespace StarConflictsRevolt.Server.WebApi.Models;

public record Structure(
    StructureVariant Variant, 
    Planet Planet,
    Guid OwnerId,
    int Health = 100,
    int MaxHealth = 100,
    bool IsOperational = true,
    DateTime? LastProductionTime = null
) : GameObject;