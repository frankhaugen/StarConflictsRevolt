# Combat System Design Document

## Star Conflicts: Revolt - Combat Mechanics

A comprehensive combat system inspired by *Star Wars: Rebellion* that handles fleet battles, planetary assaults, Death Star runs, and character missions.

---

## 🚀 **1. Fleet Combat System**

### **Combat Resolution Flow**

```csharp
public class FleetCombatSimulator
{
    public CombatResult SimulateFleetCombat(Fleet attacker, Fleet defender, Planet location)
    {
        // 1. Pre-combat setup
        var combatState = InitializeCombat(attacker, defender, location);
        
        // 2. Combat rounds
        for (int round = 1; round <= MaxRounds; round++)
        {
            // 3. Initiative phase
            var initiativeOrder = DetermineInitiative(combatState);
            
            // 4. Action phase
            foreach (var ship in initiativeOrder)
            {
                if (ship.IsDestroyed) continue;
                
                // 5. Target selection
                var target = SelectTarget(ship, combatState);
                
                // 6. Attack resolution
                var attackResult = ResolveAttack(ship, target, combatState);
                
                // 7. Apply damage
                ApplyDamage(attackResult, combatState);
            }
            
            // 8. Check for retreat/victory conditions
            if (CheckCombatEnd(combatState)) break;
        }
        
        // 9. Post-combat resolution
        return FinalizeCombat(combatState);
    }
}
```

### **Ship Combat Stats**

```csharp
public class ShipCombatStats
{
    public int Attack { get; set; }           // Base attack power
    public int Defense { get; set; }          // Base defense rating
    public int Shields { get; set; }          // Shield strength
    public int Hull { get; set; }             // Hull integrity
    public int Speed { get; set; }            // Initiative modifier
    public int Range { get; set; }            // Weapon range
    public double Accuracy { get; set; }      // Hit probability
    public List<SpecialAbility> Abilities { get; set; } = new();
}

public class SpecialAbility
{
    public string Name { get; set; } = string.Empty;
    public AbilityType Type { get; set; }
    public int Cooldown { get; set; }
    public double EffectValue { get; set; }
}

public enum AbilityType
{
    Stealth,           // Reduced detection
    IonCannon,         // Shield damage bonus
    ProtonTorpedo,     // High damage, limited ammo
    TractorBeam,       // Movement restriction
    Hyperdrive,        // Emergency retreat
    RepairDroid        // Self-healing
}
```

### **Combat Mechanics**

#### **Initiative System**
- Ships act in order based on Speed + random factor
- Faster ships get first shot advantage
- Formation bonuses affect initiative

#### **Target Selection**
- Ships target nearest enemy by default
- AI considers threat level and vulnerability
- Special abilities can force target selection

#### **Attack Resolution**
```csharp
public AttackResult ResolveAttack(Ship attacker, Ship target, CombatState state)
{
    // 1. Calculate hit probability
    var hitChance = CalculateHitChance(attacker, target, state);
    
    // 2. Roll for hit
    if (!Random.Roll(hitChance)) return AttackResult.Miss;
    
    // 3. Calculate damage
    var baseDamage = attacker.Stats.Attack;
    var damageModifiers = CalculateDamageModifiers(attacker, target, state);
    var finalDamage = baseDamage * damageModifiers;
    
    // 4. Apply to shields first, then hull
    var shieldDamage = Math.Min(finalDamage, target.Stats.Shields);
    var hullDamage = finalDamage - shieldDamage;
    
    return new AttackResult
    {
        Hit = true,
        ShieldDamage = shieldDamage,
        HullDamage = hullDamage,
        Critical = Random.Roll(0.1) // 10% critical hit chance
    };
}
```

#### **Damage System**
- Shields absorb damage first
- Hull damage reduces ship effectiveness
- Critical hits bypass shields or deal double damage
- Ships are destroyed when hull reaches 0

#### **Morale and Retreat**
- Fleet morale affects combat performance
- Ships can retreat when heavily damaged
- Fleet commander can order full retreat
- Retreating ships may be pursued

---

## 🌍 **2. Planetary Combat System**

### **Ground Combat Resolution**

```csharp
public class PlanetaryCombatSimulator
{
    public PlanetaryCombatResult SimulatePlanetaryCombat(Fleet attacker, Planet defender)
    {
        // 1. Orbital bombardment phase
        var bombardmentResult = ResolveOrbitalBombardment(attacker, defender);
        
        // 2. Ground troop deployment
        var groundForces = DeployGroundForces(attacker, defender);
        
        // 3. Ground combat resolution
        var groundCombatResult = ResolveGroundCombat(groundForces, defender);
        
        // 4. Planetary capture determination
        var captureResult = DeterminePlanetaryCapture(groundCombatResult, defender);
        
        return new PlanetaryCombatResult
        {
            BombardmentResult = bombardmentResult,
            GroundCombatResult = groundCombatResult,
            CaptureResult = captureResult
        };
    }
}
```

### **Orbital Bombardment**

```csharp
public BombardmentResult ResolveOrbitalBombardment(Fleet attacker, Planet defender)
{
    var result = new BombardmentResult();
    
    foreach (var ship in attacker.Ships.Where(s => s.CanBombardPlanets))
    {
        // Calculate bombardment damage
        var damage = ship.Stats.Attack * GetBombardmentMultiplier(ship);
        
        // Apply to planetary defenses
        if (defender.HasShieldGenerator)
        {
            var shieldAbsorption = defender.ShieldStrength * 0.5;
            damage = Math.Max(0, damage - shieldAbsorption);
        }
        
        // Apply damage to structures and population
        ApplyBombardmentDamage(damage, defender, result);
    }
    
    return result;
}
```

### **Ground Combat**

```csharp
public class GroundCombat
{
    public int AttackerTroops { get; set; }
    public int DefenderTroops { get; set; }
    public int AttackerVehicles { get; set; }
    public int DefenderVehicles { get; set; }
    public List<Structure> DefensiveStructures { get; set; } = new();
    public TerrainType Terrain { get; set; }
    public WeatherCondition Weather { get; set; }
}

public GroundCombatResult ResolveGroundCombat(GroundCombat combat, Planet planet)
{
    // 1. Calculate combat modifiers
    var terrainModifier = GetTerrainModifier(combat.Terrain);
    var weatherModifier = GetWeatherModifier(combat.Weather);
    var structureModifier = GetStructureModifier(combat.DefensiveStructures);
    
    // 2. Resolve troop combat
    var troopCasualties = CalculateTroopCasualties(combat, terrainModifier, weatherModifier);
    
    // 3. Resolve vehicle combat
    var vehicleCasualties = CalculateVehicleCasualties(combat, structureModifier);
    
    // 4. Determine victor
    var attackerStrength = combat.AttackerTroops + combat.AttackerVehicles;
    var defenderStrength = combat.DefenderTroops + combat.DefenderVehicles;
    
    return new GroundCombatResult
    {
        AttackerVictory = attackerStrength > defenderStrength,
        AttackerCasualties = troopCasualties.Attacker + vehicleCasualties.Attacker,
        DefenderCasualties = troopCasualties.Defender + vehicleCasualties.Defender
    };
}
```

### **Planetary Capture**

```csharp
public CaptureResult DeterminePlanetaryCapture(GroundCombatResult combat, Planet planet)
{
    if (!combat.AttackerVictory) return CaptureResult.DefenderHolds;
    
    // Check population resistance
    var resistanceChance = CalculateResistanceChance(planet);
    if (Random.Roll(resistanceChance))
    {
        return CaptureResult.ResistanceFormed;
    }
    
    // Check for counter-attack
    var counterAttackChance = CalculateCounterAttackChance(planet);
    if (Random.Roll(counterAttackChance))
    {
        return CaptureResult.CounterAttack;
    }
    
    return CaptureResult.Successful;
}
```

---

## ⭐ **3. Death Star Run Combat**

### **Trench Run Mechanics**

```csharp
public class DeathStarRunSimulator
{
    public DeathStarRunResult SimulateDeathStarRun(Fleet attacker, DeathStar defender)
    {
        var runState = new DeathStarRunState
        {
            Phase = RunPhase.Approach,
            TrenchPosition = 0,
            TurbolaserFire = new List<TurbolaserShot>(),
            TIEInterceptors = new List<TIEFighter>(),
            HeroPilots = attacker.Ships.Where(s => s.HasHeroPilot).ToList()
        };
        
        // Phase 1: Approach
        if (!ResolveApproachPhase(attacker, defender, runState))
            return DeathStarRunResult.ApproachFailed;
        
        // Phase 2: Trench Entry
        if (!ResolveTrenchEntry(attacker, defender, runState))
            return DeathStarRunResult.TrenchEntryFailed;
        
        // Phase 3: Trench Run
        var trenchResult = ResolveTrenchRun(attacker, defender, runState);
        if (trenchResult != TrenchRunResult.Success)
            return MapTrenchResult(trenchResult);
        
        // Phase 4: Exhaust Port Attack
        return ResolveExhaustPortAttack(attacker, defender, runState);
    }
}
```

### **Trench Run Phases**

#### **Phase 1: Approach**
- Turbolaser batteries target approaching ships
- Ships must navigate through defensive fire
- Hero pilots get evasion bonuses
- Heavy casualties expected

#### **Phase 2: Trench Entry**
- Ships must enter the trench at correct angle
- TIE Fighters launch to intercept
- Shield generator must be destroyed first
- Timing critical for success

#### **Phase 3: Trench Run**
```csharp
public TrenchRunResult ResolveTrenchRun(Fleet attacker, DeathStar defender, DeathStarRunState state)
{
    const int TrenchLength = 100;
    
    for (int position = 0; position < TrenchLength; position++)
    {
        state.TrenchPosition = position;
        
        // 1. Resolve TIE Fighter attacks
        var tieResult = ResolveTIEInterception(attacker, defender, state);
        if (tieResult == TIEInterceptionResult.AllShipsDestroyed)
            return TrenchRunResult.Destroyed;
        
        // 2. Resolve turbolaser fire
        var turbolaserResult = ResolveTurbolaserFire(attacker, defender, state);
        if (turbolaserResult == TurbolaserResult.AllShipsDestroyed)
            return TrenchRunResult.Destroyed;
        
        // 3. Check for successful shot
        if (position == TrenchLength - 1)
        {
            var shotResult = AttemptExhaustPortShot(attacker, state);
            return shotResult ? TrenchRunResult.Success : TrenchRunResult.ShotMissed;
        }
    }
    
    return TrenchRunResult.Timeout;
}
```

#### **Phase 4: Exhaust Port Attack**
- Single proton torpedo shot
- Requires precise timing and Force guidance
- Hero pilot bonuses apply
- Success destroys Death Star

### **Death Star Defenses**

```csharp
public class DeathStar
{
    public int ShieldStrength { get; set; } = 1000;
    public int TurbolaserBatteries { get; set; } = 50;
    public int TIEInterceptors { get; set; } = 100;
    public bool ShieldGeneratorDestroyed { get; set; } = false;
    public bool ExhaustPortVulnerable { get; set; } = false;
    
    public List<TurbolaserShot> GenerateTurbolaserFire()
    {
        var shots = new List<TurbolaserShot>();
        var activeBatteries = ShieldGeneratorDestroyed ? TurbolaserBatteries / 2 : TurbolaserBatteries;
        
        for (int i = 0; i < activeBatteries; i++)
        {
            if (Random.Roll(0.3)) // 30% chance to fire
            {
                shots.Add(new TurbolaserShot
                {
                    Accuracy = 0.7,
                    Damage = 50
                });
            }
        }
        
        return shots;
    }
}
```

---

## 🕵️ **4. Mission System**

### **Mission Types and Mechanics**

```csharp
public class MissionSimulator
{
    public MissionResult SimulateMission(Mission mission, Character agent, Planet target)
    {
        // 1. Calculate mission difficulty
        var difficulty = CalculateMissionDifficulty(mission, target);
        
        // 2. Apply agent skills
        var skillBonus = CalculateSkillBonus(agent, mission.Type);
        
        // 3. Apply environmental factors
        var environmentalModifier = CalculateEnvironmentalModifier(target);
        
        // 4. Calculate success probability
        var successChance = CalculateSuccessChance(difficulty, skillBonus, environmentalModifier);
        
        // 5. Resolve mission
        var success = Random.Roll(successChance);
        
        // 6. Apply consequences
        var consequences = ApplyMissionConsequences(mission, success, target);
        
        return new MissionResult
        {
            Success = success,
            Consequences = consequences,
            AgentExperience = CalculateExperience(mission, success)
        };
    }
}
```

### **Mission Types**

```csharp
public enum MissionType
{
    Diplomacy,      // Improve planetary loyalty
    Espionage,      // Gather intelligence
    Sabotage,       // Damage enemy structures
    Rescue,         // Free captured characters
    Assassination,  // Eliminate enemy character
    InciteUprising, // Start planetary rebellion
    Train,          // Improve character skills
    Investigate     // Discover hidden information
}

public class Mission
{
    public string Name { get; set; } = string.Empty;
    public MissionType Type { get; set; }
    public int Duration { get; set; } // Turns required
    public int Difficulty { get; set; } // 1-10 scale
    public List<SkillRequirement> Requirements { get; set; } = new();
    public MissionReward Reward { get; set; } = new();
    public MissionConsequence Consequence { get; set; } = new();
}
```

### **Character Skills**

```csharp
public class Character
{
    public string Name { get; set; } = string.Empty;
    public CharacterRank Rank { get; set; }
    public ForceAffinity ForceAffinity { get; set; }
    
    // Core skills
    public int Leadership { get; set; }    // Fleet command, morale
    public int Combat { get; set; }        // Personal combat, piloting
    public int Diplomacy { get; set; }     // Negotiation, persuasion
    public int Espionage { get; set; }     // Stealth, infiltration
    public int Engineering { get; set; }   // Technical skills, repair
    
    // Special abilities
    public List<SpecialAbility> Abilities { get; set; } = new();
    public List<Character> Relationships { get; set; } = new();
}

public enum CharacterRank
{
    None,
    Commander,      // Can command small fleets
    General,        // Can command large fleets
    Admiral,        // Can command entire fleets
    Hero            // Special abilities, Force sensitivity
}

public enum ForceAffinity
{
    None,
    Latent,         // Minor Force sensitivity
    Trained,        // Basic Force training
    Jedi,           // Full Jedi abilities
    Sith            // Dark side abilities
}
```

### **Mission Resolution**

```csharp
public double CalculateSuccessChance(int difficulty, double skillBonus, double environmentalModifier)
{
    var baseChance = 0.5; // 50% base chance
    var skillContribution = skillBonus * 0.3; // Skills contribute up to 30%
    var environmentalContribution = environmentalModifier * 0.2; // Environment contributes up to 20%
    
    var totalChance = baseChance + skillContribution + environmentalContribution;
    
    // Apply difficulty penalty
    var difficultyPenalty = (difficulty - 5) * 0.1; // Each difficulty level above 5 reduces by 10%
    totalChance -= Math.Max(0, difficultyPenalty);
    
    return Math.Clamp(totalChance, 0.05, 0.95); // Between 5% and 95%
}
```

---

## 🎮 **5. Integration with Game Loop**

### **Combat Integration**

```csharp
public class GameEngine
{
    public async Task ProcessCombatEvent(CombatEvent combatEvent)
    {
        switch (combatEvent.Type)
        {
            case CombatEventType.FleetCombat:
                var fleetResult = await _combatSimulator.SimulateFleetCombat(
                    combatEvent.Attacker, combatEvent.Defender, combatEvent.Location);
                await ApplyFleetCombatResult(fleetResult);
                break;
                
            case CombatEventType.PlanetaryCombat:
                var planetaryResult = await _combatSimulator.SimulatePlanetaryCombat(
                    combatEvent.Attacker, combatEvent.Defender);
                await ApplyPlanetaryCombatResult(planetaryResult);
                break;
                
            case CombatEventType.DeathStarRun:
                var deathStarResult = await _combatSimulator.SimulateDeathStarRun(
                    combatEvent.Attacker, combatEvent.Defender);
                await ApplyDeathStarRunResult(deathStarResult);
                break;
                
            case CombatEventType.Mission:
                var missionResult = await _missionSimulator.SimulateMission(
                    combatEvent.Mission, combatEvent.Agent, combatEvent.Target);
                await ApplyMissionResult(missionResult);
                break;
        }
    }
}
```

### **Event Broadcasting**

```csharp
public class CombatEventBroadcaster
{
    public async Task BroadcastCombatResult(CombatResult result)
    {
        var combatUpdate = new CombatUpdate
        {
            CombatId = result.CombatId,
            Type = result.Type,
            Winner = result.Winner,
            Casualties = result.Casualties,
            Rewards = result.Rewards,
            CinematicData = result.CinematicData
        };
        
        await _signalRHub.Clients.Group(result.SessionId)
            .SendAsync("CombatResult", combatUpdate);
    }
}
```

---

## 📊 **6. Balance Considerations**

### **Combat Balance**

- **Fleet Combat**: Large fleets should be powerful but vulnerable to smaller, specialized forces
- **Planetary Combat**: Defenders should have significant advantages, requiring substantial force to overcome
- **Death Star Run**: High risk, high reward - should be difficult but achievable with proper preparation
- **Missions**: Success should depend on character skills and mission difficulty, not random chance

### **Progression Balance**

- **Character Development**: Skills should improve through successful missions and combat
- **Technology Impact**: Advanced technology should provide significant but not overwhelming advantages
- **Resource Costs**: Combat should consume resources, making it a strategic decision
- **Time Investment**: Longer missions should provide greater rewards

### **Player Experience**

- **Predictability**: Players should be able to estimate combat outcomes
- **Agency**: Player decisions should significantly impact results
- **Drama**: Combat should feel exciting and meaningful
- **Recovery**: Defeats should be recoverable with proper strategy

---

*This combat system provides the foundation for engaging tactical gameplay while maintaining strategic depth and narrative impact.* 