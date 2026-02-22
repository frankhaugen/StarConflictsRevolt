# Mission System Design Document

## Star Conflicts: Revolt - Character Missions

A comprehensive mission system inspired by *Star Wars: Rebellion* that allows characters to undertake various missions to influence the galactic conflict.

---

## 🕵️ **1. Mission Overview**

### **Mission System Goals**

- **Character Development**: Characters gain experience and skills through successful missions
- **Strategic Impact**: Missions provide meaningful benefits to the player's faction
- **Narrative Depth**: Missions create story moments and character relationships
- **Risk vs Reward**: Higher-risk missions provide greater rewards
- **Player Agency**: Player choices significantly impact mission outcomes

### **Mission Categories**

```csharp
public enum MissionCategory
{
    Military,       // Combat-related missions
    Diplomatic,     // Negotiation and alliance building
    Espionage,      // Intelligence gathering and sabotage
    Special,        // Unique story missions
    Training        // Character development missions
}
```

---

## 👥 **2. Character System**

### **Character Creation**

```csharp
public class Character
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CharacterRank Rank { get; set; }
    public ForceAffinity ForceAffinity { get; set; }
    public Guid FactionId { get; set; }
    
    // Core attributes (1-10 scale)
    public int Leadership { get; set; }    // Fleet command, morale, influence
    public int Combat { get; set; }        // Personal combat, piloting, tactics
    public int Diplomacy { get; set; }     // Negotiation, persuasion, charm
    public int Espionage { get; set; }     // Stealth, infiltration, sabotage
    public int Engineering { get; set; }   // Technical skills, repair, hacking
    
    // Experience and progression
    public int Experience { get; set; } = 0;
    public int Level { get; set; } = 1;
    public List<SkillImprovement> SkillHistory { get; set; } = new();
    
    // Special abilities and relationships
    public List<SpecialAbility> Abilities { get; set; } = new();
    public List<CharacterRelationship> Relationships { get; set; } = new();
    public List<MissionHistory> MissionHistory { get; set; } = new();
    
    // Current status
    public CharacterStatus Status { get; set; } = CharacterStatus.Available;
    public Mission? CurrentMission { get; set; }
    public int MissionTurnsRemaining { get; set; } = 0;
}

public enum CharacterRank
{
    None,           // Basic character
    Commander,      // Can command small fleets (5-10 ships)
    General,        // Can command large fleets (10-25 ships)
    Admiral,        // Can command entire fleets (25+ ships)
    Hero            // Special abilities, Force sensitivity, unique missions
}

public enum ForceAffinity
{
    None,           // No Force sensitivity
    Latent,         // Minor Force sensitivity (intuition, luck)
    Trained,        // Basic Force training (enhanced abilities)
    Jedi,           // Full Jedi abilities (Force powers, lightsaber)
    Sith            // Dark side abilities (Force lightning, mind control)
}

public enum CharacterStatus
{
    Available,      // Ready for assignment
    OnMission,      // Currently undertaking a mission
    Wounded,        // Recovering from injuries
    Captured,       // Held by enemy forces
    Dead            // Character has died
}
```

### **Character Progression**

```csharp
public class CharacterProgression
{
    public static int CalculateLevel(int experience)
    {
        return (int)Math.Floor(Math.Sqrt(experience / 100.0)) + 1;
    }
    
    public static int ExperienceToNextLevel(int currentLevel)
    {
        return currentLevel * currentLevel * 100;
    }
    
    public static SkillImprovement CalculateSkillGain(Mission mission, bool success, Character character)
    {
        var baseGain = success ? 1 : 0;
        var difficultyBonus = mission.Difficulty * 0.2;
        var experienceBonus = character.Experience / 1000.0;
        
        var totalGain = baseGain + difficultyBonus + experienceBonus;
        
        return new SkillImprovement
        {
            Skill = GetPrimarySkill(mission.Type),
            Amount = (int)Math.Round(totalGain),
            MissionId = mission.Id,
            Success = success
        };
    }
    
    private static SkillType GetPrimarySkill(MissionType missionType)
    {
        return missionType switch
        {
            MissionType.Diplomacy => SkillType.Diplomacy,
            MissionType.Espionage => SkillType.Espionage,
            MissionType.Sabotage => SkillType.Espionage,
            MissionType.Assassination => SkillType.Combat,
            MissionType.Rescue => SkillType.Combat,
            MissionType.InciteUprising => SkillType.Diplomacy,
            MissionType.Train => SkillType.Leadership,
            MissionType.Investigate => SkillType.Espionage,
            _ => SkillType.Leadership
        };
    }
}

public enum SkillType
{
    Leadership,
    Combat,
    Diplomacy,
    Espionage,
    Engineering
}
```

---

## 🎯 **3. Mission Types**

### **Military Missions**

```csharp
public enum MilitaryMissionType
{
    Assassination,      // Eliminate enemy character
    Rescue,             // Free captured character
    Sabotage,           // Damage enemy structures/fleets
    Infiltration,       // Gather military intelligence
    Training,           // Improve character skills
    Recruitment         // Recruit new characters
}

public class MilitaryMission : Mission
{
    public MilitaryMissionType MilitaryType { get; set; }
    public Character? TargetCharacter { get; set; }
    public List<Structure>? TargetStructures { get; set; }
    public Fleet? TargetFleet { get; set; }
    
    public override double CalculateSuccessChance(Character agent, Planet target)
    {
        var baseChance = 0.4; // Lower base chance for military missions
        var combatBonus = agent.Combat * 0.05;
        var stealthBonus = agent.Espionage * 0.03;
        var difficultyPenalty = (Difficulty - 5) * 0.08;
        
        return Math.Clamp(baseChance + combatBonus + stealthBonus - difficultyPenalty, 0.05, 0.9);
    }
}
```

### **Diplomatic Missions**

```csharp
public enum DiplomaticMissionType
{
    NegotiateAlliance,  // Form alliance with neutral faction
    ImproveRelations,   // Increase planetary loyalty
    InciteUprising,     // Start rebellion on enemy planet
    MediateConflict,    // Resolve conflict between factions
    TradeAgreement,     // Establish trade relations
    CulturalExchange    // Improve cultural understanding
}

public class DiplomaticMission : Mission
{
    public DiplomaticMissionType DiplomaticType { get; set; }
    public Faction? TargetFaction { get; set; }
    public int LoyaltyChange { get; set; }
    public List<ResourceType>? TradeResources { get; set; }
    
    public override double CalculateSuccessChance(Character agent, Planet target)
    {
        var baseChance = 0.6; // Higher base chance for diplomatic missions
        var diplomacyBonus = agent.Diplomacy * 0.06;
        var leadershipBonus = agent.Leadership * 0.03;
        var difficultyPenalty = (Difficulty - 5) * 0.06;
        
        return Math.Clamp(baseChance + diplomacyBonus + leadershipBonus - difficultyPenalty, 0.1, 0.95);
    }
}
```

### **Espionage Missions**

```csharp
public enum EspionageMissionType
{
    GatherIntelligence, // Learn enemy plans/resources
    StealTechnology,    // Acquire enemy technology
    PlantDisinformation, // Spread false information
    HackSystems,        // Disable enemy systems
    InfiltrateNetwork,  // Gain access to enemy communications
    EstablishSpyNetwork // Create long-term intelligence network
}

public class EspionageMission : Mission
{
    public EspionageMissionType EspionageType { get; set; }
    public Technology? TargetTechnology { get; set; }
    public List<Structure>? TargetSystems { get; set; }
    public bool RequiresStealth { get; set; } = true;
    
    public override double CalculateSuccessChance(Character agent, Planet target)
    {
        var baseChance = 0.5;
        var espionageBonus = agent.Espionage * 0.07;
        var engineeringBonus = agent.Engineering * 0.02;
        var difficultyPenalty = (Difficulty - 5) * 0.07;
        
        // Stealth missions are harder
        if (RequiresStealth)
        {
            baseChance -= 0.1;
        }
        
        return Math.Clamp(baseChance + espionageBonus + engineeringBonus - difficultyPenalty, 0.05, 0.9);
    }
}
```

### **Special Missions**

```csharp
public enum SpecialMissionType
{
    ForceTraining,      // Jedi/Sith training mission
    ArtifactRecovery,   // Find Force artifacts
    ProphecyInvestigation, // Investigate Force prophecies
    DeathStarPlans,     // Steal Death Star plans
    HeroicRescue,       // High-risk rescue mission
    LegendaryBattle     // Epic combat mission
}

public class SpecialMission : Mission
{
    public SpecialMissionType SpecialType { get; set; }
    public bool RequiresForceSensitivity { get; set; } = false;
    public ForceAffinity MinimumForceLevel { get; set; } = ForceAffinity.None;
    public List<SpecialReward> SpecialRewards { get; set; } = new();
    
    public override double CalculateSuccessChance(Character agent, Planet target)
    {
        var baseChance = 0.3; // Lower base chance for special missions
        var skillBonus = GetHighestSkill(agent) * 0.04;
        var forceBonus = GetForceBonus(agent);
        var difficultyPenalty = (Difficulty - 5) * 0.1;
        
        return Math.Clamp(baseChance + skillBonus + forceBonus - difficultyPenalty, 0.05, 0.8);
    }
    
    private double GetForceBonus(Character agent)
    {
        return agent.ForceAffinity switch
        {
            ForceAffinity.Latent => 0.05,
            ForceAffinity.Trained => 0.1,
            ForceAffinity.Jedi => 0.2,
            ForceAffinity.Sith => 0.2,
            _ => 0
        };
    }
}
```

---

## 🎁 **4. Mission Rewards**

### **Reward Types**

```csharp
public abstract class MissionReward
{
    public string Description { get; set; } = string.Empty;
    public RewardType Type { get; set; }
    public int Value { get; set; }
}

public class ResourceReward : MissionReward
{
    public ResourceType ResourceType { get; set; }
    public int Amount { get; set; }
}

public class TechnologyReward : MissionReward
{
    public Technology Technology { get; set; } = new();
    public bool IsBlueprint { get; set; } = false; // Blueprint vs instant research
}

public class CharacterReward : MissionReward
{
    public Character Character { get; set; } = new();
    public bool IsRecruitment { get; set; } = true; // Recruit vs rescue
}

public class IntelligenceReward : MissionReward
{
    public IntelligenceType IntelligenceType { get; set; }
    public string Information { get; set; } = string.Empty;
    public int TurnsValid { get; set; } = 5;
}

public class LoyaltyReward : MissionReward
{
    public Guid PlanetId { get; set; }
    public int LoyaltyChange { get; set; }
    public bool IsPermanent { get; set; } = false;
}

public enum RewardType
{
    Resources,
    Technology,
    Character,
    Intelligence,
    Loyalty,
    Experience,
    Special
}

public enum IntelligenceType
{
    FleetLocations,
    ResourceStockpiles,
    TechnologyLevels,
    CharacterLocations,
    StrategicPlans,
    WeaknessExploits
}
```

### **Reward Calculation**

```csharp
public class MissionRewardCalculator
{
    public static List<MissionReward> CalculateRewards(Mission mission, bool success, Character agent)
    {
        var rewards = new List<MissionReward>();
        
        if (!success)
        {
            // Failed missions still provide some experience
            rewards.Add(new ExperienceReward
            {
                Description = "Experience from failed mission",
                Type = RewardType.Experience,
                Value = mission.Difficulty * 10
            });
            return rewards;
        }
        
        // Base rewards
        rewards.AddRange(mission.BaseRewards);
        
        // Difficulty bonuses
        var difficultyBonus = mission.Difficulty * 0.2;
        
        // Character skill bonuses
        var skillBonus = CalculateSkillBonus(agent, mission.Type);
        
        // Special rewards for critical success
        if (Random.Roll(0.1)) // 10% chance for critical success
        {
            rewards.AddRange(mission.CriticalRewards);
        }
        
        // Apply bonuses to resource rewards
        foreach (var reward in rewards.OfType<ResourceReward>())
        {
            reward.Amount = (int)(reward.Amount * (1 + difficultyBonus + skillBonus));
        }
        
        return rewards;
    }
    
    private static double CalculateSkillBonus(Character agent, MissionType missionType)
    {
        var primarySkill = GetPrimarySkill(missionType);
        var skillLevel = GetSkillLevel(agent, primarySkill);
        
        return skillLevel * 0.1; // Each skill level adds 10% bonus
    }
}
```

---

## ⚡ **5. Mission Execution**

### **Mission Assignment**

```csharp
public class MissionAssignmentService
{
    public async Task<MissionAssignmentResult> AssignMission(Mission mission, Character agent, Planet target)
    {
        // Validate assignment
        var validation = ValidateMissionAssignment(mission, agent, target);
        if (!validation.IsValid)
        {
            return new MissionAssignmentResult
            {
                Success = false,
                ErrorMessage = validation.ErrorMessage
            };
        }
        
        // Update character status
        agent.Status = CharacterStatus.OnMission;
        agent.CurrentMission = mission;
        agent.MissionTurnsRemaining = mission.Duration;
        
        // Create mission event
        var missionEvent = new MissionStartedEvent
        {
            MissionId = mission.Id,
            AgentId = agent.Id,
            TargetPlanetId = target.Id,
            StartTurn = GetCurrentTurn(),
            ExpectedCompletionTurn = GetCurrentTurn() + mission.Duration
        };
        
        await _eventStore.AppendEvent(missionEvent);
        
        return new MissionAssignmentResult
        {
            Success = true,
            MissionEvent = missionEvent
        };
    }
    
    private MissionValidation ValidateMissionAssignment(Mission mission, Character agent, Planet target)
    {
        // Check if agent is available
        if (agent.Status != CharacterStatus.Available)
        {
            return new MissionValidation { IsValid = false, ErrorMessage = "Agent is not available" };
        }
        
        // Check skill requirements
        if (!CheckSkillRequirements(mission, agent))
        {
            return new MissionValidation { IsValid = false, ErrorMessage = "Agent lacks required skills" };
        }
        
        // Check if target is valid
        if (!IsValidTarget(mission, target))
        {
            return new MissionValidation { IsValid = false, ErrorMessage = "Invalid mission target" };
        }
        
        return new MissionValidation { IsValid = true };
    }
}
```

### **Mission Resolution**

```csharp
public class MissionResolutionService
{
    public async Task<MissionResult> ResolveMission(Mission mission, Character agent, Planet target)
    {
        // Calculate success probability
        var successChance = mission.CalculateSuccessChance(agent, target);
        
        // Determine success/failure
        var success = Random.Roll(successChance);
        
        // Calculate rewards
        var rewards = MissionRewardCalculator.CalculateRewards(mission, success, agent);
        
        // Apply consequences
        var consequences = await ApplyMissionConsequences(mission, success, target);
        
        // Update character
        await UpdateCharacterAfterMission(agent, mission, success);
        
        // Create result
        var result = new MissionResult
        {
            MissionId = mission.Id,
            AgentId = agent.Id,
            TargetPlanetId = target.Id,
            Success = success,
            Rewards = rewards,
            Consequences = consequences,
            ExperienceGained = CalculateExperienceGain(mission, success, agent)
        };
        
        // Broadcast result
        await BroadcastMissionResult(result);
        
        return result;
    }
    
    private async Task<List<MissionConsequence>> ApplyMissionConsequences(Mission mission, bool success, Planet target)
    {
        var consequences = new List<MissionConsequence>();
        
        if (success)
        {
            // Apply positive consequences
            consequences.AddRange(mission.SuccessConsequences);
        }
        else
        {
            // Apply negative consequences
            consequences.AddRange(mission.FailureConsequences);
            
            // Check for agent capture/death
            if (Random.Roll(0.2)) // 20% chance of capture on failure
            {
                consequences.Add(new AgentCapturedConsequence
                {
                    AgentId = mission.AssignedAgent?.Id ?? Guid.Empty,
                    CapturingFactionId = target.OwnerId ?? Guid.Empty
                });
            }
        }
        
        return consequences;
    }
}
```

---

## 📊 **6. Mission Balance**

### **Difficulty Scaling**

```csharp
public class MissionDifficultyCalculator
{
    public static int CalculateMissionDifficulty(Mission mission, Planet target, Character agent)
    {
        var baseDifficulty = mission.BaseDifficulty;
        
        // Target planet factors
        var planetModifier = CalculatePlanetModifier(target);
        
        // Agent skill factors
        var skillModifier = CalculateSkillModifier(agent, mission.Type);
        
        // Environmental factors
        var environmentalModifier = CalculateEnvironmentalModifier(target);
        
        // Time pressure
        var timeModifier = CalculateTimeModifier(mission.Duration);
        
        var finalDifficulty = baseDifficulty + planetModifier + skillModifier + 
                             environmentalModifier + timeModifier;
        
        return Math.Clamp((int)Math.Round(finalDifficulty), 1, 10);
    }
    
    private static double CalculatePlanetModifier(Planet planet)
    {
        var modifier = 0.0;
        
        // Enemy-controlled planets are harder
        if (planet.OwnerId != null)
        {
            modifier += 2.0;
        }
        
        // Planets with defensive structures are harder
        modifier += planet.Structures.Count * 0.5;
        
        // Shield generators make missions much harder
        if (planet.Structures.Any(s => s.Variant == StructureVariant.ShieldGenerator))
        {
            modifier += 1.5;
        }
        
        return modifier;
    }
}
```

### **Risk vs Reward**

- **Low Risk Missions**: 60-80% success chance, small rewards
- **Medium Risk Missions**: 40-60% success chance, moderate rewards
- **High Risk Missions**: 20-40% success chance, large rewards
- **Critical Missions**: 10-30% success chance, game-changing rewards

### **Progression Balance**

- **Character Development**: Skills improve through successful missions
- **Mission Availability**: Higher-level missions unlock as characters progress
- **Resource Investment**: Missions consume time and potentially resources
- **Strategic Impact**: Successful missions provide meaningful advantages

---

## 🎮 **7. Integration with Game Systems**

### **Mission Events**

```csharp
public class MissionEvent : IGameEvent
{
    public Guid MissionId { get; set; }
    public Guid AgentId { get; set; }
    public Guid TargetPlanetId { get; set; }
    public MissionEventType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum MissionEventType
{
    MissionStarted,
    MissionCompleted,
    MissionFailed,
    AgentCaptured,
    AgentRescued,
    IntelligenceGathered,
    TechnologyStolen,
    AllianceFormed
}
```

### **UI Integration**

```csharp
public class MissionUI
{
    public async Task ShowMissionAssignment(Mission mission, Character agent, Planet target)
    {
        var assignmentView = new MissionAssignmentView
        {
            Mission = mission,
            Agent = agent,
            Target = target,
            SuccessChance = mission.CalculateSuccessChance(agent, target),
            ExpectedRewards = mission.BaseRewards,
            RiskLevel = CalculateRiskLevel(mission)
        };
        
        await _uiManager.ShowView(assignmentView);
    }
    
    public async Task ShowMissionResult(MissionResult result)
    {
        var resultView = new MissionResultView
        {
            Result = result,
            CinematicData = GenerateCinematicData(result),
            Rewards = result.Rewards,
            Consequences = result.Consequences
        };
        
        await _uiManager.ShowView(resultView);
    }
}
```

---

*This mission system provides deep character progression, meaningful strategic choices, and engaging narrative moments that enhance the overall game experience.* 