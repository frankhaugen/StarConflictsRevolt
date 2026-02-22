namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;

public class Ship : Infrastructure.Datastore.Entities.GameObject
{
    public Ship(Guid parse, string xWing, bool b)
    {
        Id = parse;
        Model = xWing;
        IsUnderConstruction = b;
    }

    public Ship()
    {
        // Default constructor for serialization
    }

    public string Model { get; set; } = string.Empty;
    public bool IsUnderConstruction { get; set; }
}