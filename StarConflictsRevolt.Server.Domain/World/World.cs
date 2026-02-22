using StarConflictsRevolt.Server.Domain.Galaxies;
using StarConflictsRevolt.Server.Domain.Stars;

namespace StarConflictsRevolt.Server.Domain.World;

public class World
{
    public World()
    {
        Id = Guid.Empty;
        Galaxy = new Galaxy(new List<StarSystem>());
        Players = new List<IPlayerController>();
        CreatedAt = DateTime.UtcNow;
        LastUpdated = null;
        IsActive = true;
    }

    public World(Guid id, Galaxy galaxy, List<IPlayerController>? players = null, DateTime? createdAt = null, DateTime? lastUpdated = null, bool isActive = true)
    {
        Id = id;
        Galaxy = galaxy;
        Players = players ?? new List<IPlayerController>();
        CreatedAt = createdAt ?? DateTime.UtcNow;
        LastUpdated = lastUpdated;
        IsActive = isActive;
    }

    public Guid Id { get; set; }
    public Galaxy Galaxy { get; set; }
    public List<IPlayerController> Players { get; set; }
    public Dictionary<Guid, Players.PlayerState> PlayerStates { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdated { get; set; }
    public bool IsActive { get; set; }
}