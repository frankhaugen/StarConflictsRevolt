using StarConflictsRevolt.Server.WebApi.Services;

namespace StarConflictsRevolt.Server.WebApi.Models;

public class World
{
    public Guid Id { get; set; }
    public Galaxy Galaxy { get; set; }
    public List<PlayerController> Players { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdated { get; set; }
    public bool IsActive { get; set; }

    public World()
    {
        Id = Guid.Empty;
        Galaxy = new Galaxy(new List<StarSystem>());
        Players = new List<PlayerController>();
        CreatedAt = DateTime.UtcNow;
        LastUpdated = null;
        IsActive = true;
    }

    public World(Guid id, Galaxy galaxy, List<PlayerController>? players = null, DateTime? createdAt = null, DateTime? lastUpdated = null, bool isActive = true)
    {
        Id = id;
        Galaxy = galaxy;
        Players = players ?? new List<PlayerController>();
        CreatedAt = createdAt ?? DateTime.UtcNow;
        LastUpdated = lastUpdated;
        IsActive = isActive;
    }
}