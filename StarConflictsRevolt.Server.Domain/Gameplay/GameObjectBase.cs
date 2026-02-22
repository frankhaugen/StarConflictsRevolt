namespace StarConflictsRevolt.Server.Domain.Gameplay;

/// <summary>
/// Base class for entity types that are classes (not records). Use this for Gameplay.World, Galaxy, Fleet, etc.
/// For record-based domain types (e.g. Galaxies.Galaxy, Planets.Planet) use the abstract record GameObject.
/// </summary>
public abstract class GameObjectBase
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}
