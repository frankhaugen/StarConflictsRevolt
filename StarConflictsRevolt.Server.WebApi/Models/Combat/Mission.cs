namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class Mission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MissionType Type { get; set; }
    public int BaseDifficulty { get; set; }
    public int Duration { get; set; } // in turns
    public List<MissionRequirement> Requirements { get; set; } = new();
    public List<MissionReward> PotentialRewards { get; set; } = new();
    public List<MissionConsequence> PotentialConsequences { get; set; } = new();
    public bool IsAvailable { get; set; } = true;
    public Guid? AssignedAgentId { get; set; }
    public Guid? TargetPlanetId { get; set; }
    public MissionStatus Status { get; set; } = MissionStatus.Available;
    public DateTime? StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
}

public enum MissionType
{
    Diplomacy,
    Espionage,
    Sabotage,
    Rescue,
    Assassination,
    Infiltration,
    Reconnaissance,
    Smuggling,
    BountyHunting,
    Training
}

public enum MissionStatus
{
    Available,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

public class MissionRequirement
{
    public RequirementType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public bool IsMet { get; set; } = false;
}

public enum RequirementType
{
    CharacterLevel,
    CharacterSkill,
    CharacterType,
    ResourceCost,
    TechnologyLevel,
    ReputationLevel
}

public class MissionReward
{
    public RewardType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public double Probability { get; set; } = 1.0; // Chance of receiving this reward
    public Guid? TargetId { get; set; }
}

public class MissionConsequence
{
    public ConsequenceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public double Probability { get; set; } = 1.0; // Chance of this consequence occurring
    public Guid? TargetId { get; set; }
}

public class Character
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public CharacterType Type { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public Guid OwnerId { get; set; }
    
    // Core stats
    public int Leadership { get; set; } = 10;
    public int Combat { get; set; } = 10;
    public int Diplomacy { get; set; } = 10;
    public int Espionage { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    
    // Special abilities
    public List<CharacterAbility> Abilities { get; set; } = new();
    public bool IsForceSensitive { get; set; } = false;
    public ForceAlignment ForceAlignment { get; set; } = ForceAlignment.None;
    
    // Current state
    public bool IsAlive { get; set; } = true;
    public bool IsAvailable { get; set; } = true;
    public Guid? CurrentMissionId { get; set; }
    public int Loyalty { get; set; } = 100;
    
    // Relationships
    public List<CharacterRelationship> Relationships { get; set; } = new();
    
    public int GetSkillForMission(MissionType missionType)
    {
        return missionType switch
        {
            MissionType.Diplomacy => Diplomacy,
            MissionType.Espionage => Espionage,
            MissionType.Sabotage => Espionage + Combat,
            MissionType.Rescue => Combat + Leadership,
            MissionType.Assassination => Combat + Espionage,
            MissionType.Infiltration => Espionage + Intelligence,
            MissionType.Reconnaissance => Intelligence + Espionage,
            MissionType.Smuggling => Espionage + Diplomacy,
            MissionType.BountyHunting => Combat + Intelligence,
            MissionType.Training => Leadership + Intelligence,
            _ => (Leadership + Combat + Diplomacy + Espionage + Intelligence) / 5
        };
    }
    
    public void GainExperience(int amount)
    {
        Experience += amount;
        var experienceForNextLevel = Level * 100;
        
        if (Experience >= experienceForNextLevel)
        {
            Level++;
            Experience -= experienceForNextLevel;
            
            // Increase stats based on character type
            IncreaseStatsOnLevelUp();
        }
    }
    
    private void IncreaseStatsOnLevelUp()
    {
        switch (Type)
        {
            case CharacterType.Diplomat:
                Diplomacy += 2;
                Leadership += 1;
                break;
            case CharacterType.Warrior:
                Combat += 2;
                Leadership += 1;
                break;
            case CharacterType.Spy:
                Espionage += 2;
                Intelligence += 1;
                break;
            case CharacterType.Commander:
                Leadership += 2;
                Combat += 1;
                break;
            case CharacterType.Scientist:
                Intelligence += 2;
                Espionage += 1;
                break;
            case CharacterType.Jedi:
                if (IsForceSensitive)
                {
                    Combat += 2;
                    Leadership += 1;
                    Intelligence += 1;
                }
                break;
            case CharacterType.Sith:
                if (IsForceSensitive)
                {
                    Combat += 2;
                    Espionage += 1;
                    Intelligence += 1;
                }
                break;
        }
    }
}

public enum CharacterType
{
    Diplomat,
    Warrior,
    Spy,
    Commander,
    Scientist,
    Jedi,
    Sith,
    BountyHunter,
    Smuggler,
    Politician
}

public enum ForceAlignment
{
    None,
    Light,
    Dark,
    Gray
}

public class CharacterAbility
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AbilityEffect Effect { get; set; }
    public int Cooldown { get; set; } = 0;
    public int CurrentCooldown { get; set; } = 0;
    public bool IsActive { get; set; } = false;
    
    public bool CanActivate() => CurrentCooldown <= 0 && !IsActive;
    
    public void Activate()
    {
        if (CanActivate())
        {
            IsActive = true;
            CurrentCooldown = Cooldown;
        }
    }
    
    public void UpdateCooldown()
    {
        if (CurrentCooldown > 0)
            CurrentCooldown--;
    }
}

public class AbilityEffect
{
    public EffectType Type { get; set; }
    public string TargetStat { get; set; } = string.Empty;
    public double Modifier { get; set; } = 1.0;
    public int Duration { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
}

public enum EffectType
{
    StatBoost,
    SkillBonus,
    DamageReduction,
    AccuracyBonus,
    StealthBonus,
    Healing,
    MoraleBoost
}

public class CharacterRelationship
{
    public Guid CharacterId { get; set; }
    public RelationshipType Type { get; set; }
    public int Strength { get; set; } = 0; // -100 to +100
    public string Description { get; set; } = string.Empty;
}

public enum RelationshipType
{
    Ally,
    Enemy,
    Mentor,
    Student,
    Rival,
    Friend,
    Lover,
    Family
} 