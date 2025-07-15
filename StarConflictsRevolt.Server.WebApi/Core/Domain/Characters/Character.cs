namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Characters;

public class Character : Datastore.Entities.GameObject
{
    public string Name { get; set; } = string.Empty;
    public CharacterRank Rank { get; set; } = CharacterRank.None;
    public ForceAffinity ForceAffinity { get; set; } = ForceAffinity.None;
    public Guid OwnerId { get; set; }
    
    // Character stats
    public int Leadership { get; set; } = 1;
    public int Combat { get; set; } = 1;
    public int Diplomacy { get; set; } = 1;
    public int Espionage { get; set; } = 1;
    
    // Experience and progression
    public int Experience { get; set; } = 0;
    public int Level { get; set; } = 1;
    
    // Current status
    public bool IsAlive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public Guid? CurrentMissionId { get; set; }
    public DateTime? MissionStartTime { get; set; }
    
    // Relationships and loyalty
    public Dictionary<Guid, int> Relationships { get; set; } = new(); // CharacterId -> Relationship value (-100 to 100)
    public int Loyalty { get; set; } = 50; // 0-100, higher is more loyal
    
    // Special abilities
    public List<string> Abilities { get; set; } = new();
    public List<string> ForcePowers { get; set; } = new();
    
    public Character()
    {
        // Default constructor for serialization
    }
    
    public Character(string name, CharacterRank rank, ForceAffinity forceAffinity, Guid ownerId)
    {
        Name = name;
        Rank = rank;
        ForceAffinity = forceAffinity;
        OwnerId = ownerId;
        Id = Guid.NewGuid();
    }
    
    public void GainExperience(int amount)
    {
        Experience += amount;
        
        // Level up logic
        var requiredExp = Level * 100;
        if (Experience >= requiredExp)
        {
            Level++;
            Experience -= requiredExp;
            
            // Increase stats on level up
            Leadership += Random.Shared.Next(0, 2);
            Combat += Random.Shared.Next(0, 2);
            Diplomacy += Random.Shared.Next(0, 2);
            Espionage += Random.Shared.Next(0, 2);
        }
    }
    
    public bool CanUseForcePower(string powerName)
    {
        return ForceAffinity != ForceAffinity.None && ForcePowers.Contains(powerName);
    }
    
    public int GetRelationship(Guid otherCharacterId)
    {
        return Relationships.TryGetValue(otherCharacterId, out var value) ? value : 0;
    }
    
    public void ModifyRelationship(Guid otherCharacterId, int change)
    {
        if (Relationships.ContainsKey(otherCharacterId))
        {
            Relationships[otherCharacterId] = Math.Clamp(Relationships[otherCharacterId] + change, -100, 100);
        }
        else
        {
            Relationships[otherCharacterId] = Math.Clamp(change, -100, 100);
        }
    }
    
    public bool IsForceSensitive => ForceAffinity != ForceAffinity.None;
    public bool IsJedi => ForceAffinity == ForceAffinity.Jedi;
    public bool IsSith => ForceAffinity == ForceAffinity.Sith;
    public bool IsTrained => ForceAffinity == ForceAffinity.Trained;
} 