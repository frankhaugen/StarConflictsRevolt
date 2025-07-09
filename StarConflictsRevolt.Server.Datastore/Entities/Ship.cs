namespace StarConflictsRevolt.Server.Datastore.Entities;

public class Ship : GameObject
{
    public Ship(Guid parse, string xWing, HyperdriveRating hyperdriveRating, bool b)
    {
        Id = parse;
        Model = xWing;
        Hyperdrive = hyperdriveRating;
        IsUnderConstruction = b;
    }

    public Ship()
    {
        // Default constructor for serialization
    }

    public string Model { get; set; }
    public HyperdriveRating Hyperdrive { get; set; }
    public bool IsUnderConstruction { get; set; }
} 