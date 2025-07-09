namespace StarConflictsRevolt.Server.Core.Models;

public record Ship(
    Guid Id,
    string Model,
    bool IsUnderConstruction
) : GameObject;