using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Galaxies;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Stars;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.World;

public class World
{
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

    public Guid Id { get; set; }
    public Galaxy Galaxy { get; set; }
    public List<PlayerController> Players { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdated { get; set; }
    public bool IsActive { get; set; }
}