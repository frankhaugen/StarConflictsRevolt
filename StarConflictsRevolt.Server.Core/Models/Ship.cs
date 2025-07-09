namespace StarConflictsRevolt.Server.Core.Models;

public record Ship(
    Guid Id,
    string Model,
    HyperdriveRating Hyperdrive,
    bool IsUnderConstruction
) : GameObject;