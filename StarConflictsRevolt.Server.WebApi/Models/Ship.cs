namespace StarConflictsRevolt.Server.WebApi.Models;

public record Ship(
    Guid Id,
    string Model,
    bool IsUnderConstruction
);