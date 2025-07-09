namespace StarConflictsRevolt.Server.Datastore.Entities;

public class HyperdriveRating
{
    public HyperdriveRating(float f, float f1)
    {
    }
    
    public HyperdriveRating()
    {
        // Default constructor for serialization
    }

    public float Current { get; set; }
    public float Optimal { get; set; }
}