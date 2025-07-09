namespace StarConflictsRevolt.Server.Core;

public record HyperdriveRating(float Current, float Optimal);

public record Ship(
    Guid Id,
    string Model,
    HyperdriveRating Hyperdrive,
    bool IsUnderConstruction
) : GameObject
{
    public Ship() : this(Guid.Empty, string.Empty, new HyperdriveRating(1.0f, 1.0f), false) { }
} 