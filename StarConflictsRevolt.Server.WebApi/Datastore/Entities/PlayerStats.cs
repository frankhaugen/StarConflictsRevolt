using StarConflictsRevolt.Server.Core.Models;

namespace StarConflictsRevolt.Server.Datastore.Entities;

public class PlayerStats : IGameObject
{
    /// <inheritdoc />
    public Guid Id { get; set; }
    
    public Guid PlayerId { get; set; }
    
    public Guid SessionId { get; set; }
    
    public string PlayerName { get; set; } = string.Empty;
    
    public int FleetsOwned { get; set; }
    
    public int PlanetsControlled { get; set; }
    
    public int StructuresBuilt { get; set; }
    
    public int BattlesWon { get; set; }
    
    public int BattlesLost { get; set; }
    
    public DateTime LastUpdated { get; set; }
    
    public DateTime Created { get; set; }
} 