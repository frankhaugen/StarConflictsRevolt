using StarConflictsRevolt.Server.WebApi.Core.Domain.Characters;

namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Missions;

public class Mission : Datastore.Entities.GameObject
{
    public string Name { get; set; } = string.Empty;
    public MissionType Type { get; set; }
    public MissionStatus Status { get; set; } = MissionStatus.Pending;
    public Guid OwnerId { get; set; }
    public Guid? AssignedCharacterId { get; set; }
    public Guid TargetPlanetId { get; set; }
    
    // Mission requirements
    public int RequiredLeadership { get; set; } = 1;
    public int RequiredCombat { get; set; } = 1;
    public int RequiredDiplomacy { get; set; } = 1;
    public int RequiredEspionage { get; set; } = 1;
    public List<string> RequiredAbilities { get; set; } = new();
    public List<string> RequiredForcePowers { get; set; } = new();
    
    // Mission timing
    public DateTime StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    public DateTime? CompletionTime { get; set; }
    
    // Success/failure conditions
    public double SuccessChance { get; set; } = 0.5;
    public double CriticalSuccessChance { get; set; } = 0.1;
    public double CriticalFailureChance { get; set; } = 0.1;
    
    // Rewards and consequences
    public List<MissionReward> Rewards { get; set; } = new();
    public List<MissionConsequence> Consequences { get; set; } = new();
    
    // Mission-specific data
    public Dictionary<string, object> MissionData { get; set; } = new();
    
    public Mission()
    {
        // Default constructor for serialization
    }
    
    public Mission(string name, MissionType type, Guid ownerId, Guid targetPlanetId, TimeSpan duration)
    {
        Name = name;
        Type = type;
        OwnerId = ownerId;
        TargetPlanetId = targetPlanetId;
        Duration = duration;
        StartTime = DateTime.UtcNow;
        Id = Guid.NewGuid();
    }
    
    public bool CanCharacterComplete(Character character)
    {
        if (character.Leadership < RequiredLeadership) return false;
        if (character.Combat < RequiredCombat) return false;
        if (character.Diplomacy < RequiredDiplomacy) return false;
        if (character.Espionage < RequiredEspionage) return false;
        
        foreach (var ability in RequiredAbilities)
        {
            if (!character.Abilities.Contains(ability)) return false;
        }
        
        foreach (var forcePower in RequiredForcePowers)
        {
            if (!character.ForcePowers.Contains(forcePower)) return false;
        }
        
        return true;
    }
    
    public double CalculateSuccessChance(Character character)
    {
        if (!CanCharacterComplete(character)) return 0.0;
        
        var baseChance = SuccessChance;
        
        // Bonus from exceeding requirements
        baseChance += (character.Leadership - RequiredLeadership) * 0.05;
        baseChance += (character.Combat - RequiredCombat) * 0.05;
        baseChance += (character.Diplomacy - RequiredDiplomacy) * 0.05;
        baseChance += (character.Espionage - RequiredEspionage) * 0.05;
        
        // Bonus from character level
        baseChance += (character.Level - 1) * 0.02;
        
        // Bonus from Force sensitivity
        if (character.IsForceSensitive) baseChance += 0.1;
        if (character.IsJedi || character.IsSith) baseChance += 0.15;
        
        return Math.Clamp(baseChance, 0.0, 1.0);
    }
    
    public bool IsCompleted => Status == MissionStatus.Completed || Status == MissionStatus.Failed;
    public bool IsInProgress => Status == MissionStatus.InProgress;
    public bool IsPending => Status == MissionStatus.Pending;
    
    public TimeSpan TimeRemaining
    {
        get
        {
            if (!IsInProgress) return TimeSpan.Zero;
            var elapsed = DateTime.UtcNow - StartTime;
            return Duration - elapsed;
        }
    }
    
    public bool IsOverdue => IsInProgress && TimeRemaining <= TimeSpan.Zero;
} 