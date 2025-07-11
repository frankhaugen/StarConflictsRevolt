namespace StarConflictsRevolt.Server.WebApi.Datastore.Entities;

public class Ship : GameObject
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

    public string Model { get; set; }
    public bool IsUnderConstruction { get; set; }
} 