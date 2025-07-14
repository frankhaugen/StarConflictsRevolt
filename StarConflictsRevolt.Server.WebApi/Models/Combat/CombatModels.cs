namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

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
    
    // Current combat state
    public int CurrentShields { get; set; }
    public int CurrentHull { get; set; }
    public bool IsDestroyed => CurrentHull <= 0;
    public bool IsDisabled => CurrentHull <= CurrentHull / 4; // 25% hull remaining
    
    public void InitializeCombat()
    {
        CurrentShields = Shields;
        CurrentHull = Hull;
    }
    
    public void ApplyDamage(int shieldDamage, int hullDamage)
    {
        CurrentShields = Math.Max(0, CurrentShields - shieldDamage);
        CurrentHull = Math.Max(0, CurrentHull - hullDamage);
    }
    
    public double GetCombatEffectiveness()
    {
        if (IsDestroyed) return 0.0;
        
        var shieldEffectiveness = CurrentShields / (double)Shields;
        var hullEffectiveness = CurrentHull / (double)Hull;
        
        return (shieldEffectiveness + hullEffectiveness) / 2.0;
    }
}

public class SpecialAbility
{
    public string Name { get; set; } = string.Empty;
    public AbilityType Type { get; set; }
    public int Cooldown { get; set; }
    public int CurrentCooldown { get; set; } = 0;
    public double EffectValue { get; set; }
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

public enum AbilityType
{
    Stealth,           // Reduced detection
    IonCannon,         // Shield damage bonus
    ProtonTorpedo,     // High damage, limited ammo
    TractorBeam,       // Movement restriction
    Hyperdrive,        // Emergency retreat
    RepairDroid        // Self-healing
}

public class AttackResult
{
    public bool Hit { get; set; }
    public int ShieldDamage { get; set; }
    public int HullDamage { get; set; }
    public bool Critical { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public static AttackResult Miss => new() { Hit = false, Description = "Attack missed" };
    
    public static AttackResult CreateHit(int shieldDamage, int hullDamage, bool critical = false)
    {
        return new AttackResult
        {
            Hit = true,
            ShieldDamage = shieldDamage,
            HullDamage = hullDamage,
            Critical = critical,
            Description = critical ? "Critical hit!" : "Attack hit"
        };
    }
}

public class CombatState
{
    public Guid CombatId { get; set; } = Guid.NewGuid();
    public CombatType Type { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public int CurrentRound { get; set; } = 0;
    public int MaxRounds { get; set; } = 20;
    
    // Combatants
    public List<CombatShip> AttackerShips { get; set; } = new();
    public List<CombatShip> DefenderShips { get; set; } = new();
    
    // Combat environment
    public Planet? Location { get; set; }
    public CombatEnvironment Environment { get; set; } = new();
    
    // Combat results
    public List<CombatRound> Rounds { get; set; } = new();
    public CombatResult? FinalResult { get; set; }
    
    public bool IsCombatEnded => FinalResult != null || CurrentRound >= MaxRounds;
    
    public List<CombatShip> GetAllShips()
    {
        return AttackerShips.Concat(DefenderShips).ToList();
    }
    
    public List<CombatShip> GetActiveShips()
    {
        return GetAllShips().Where(s => !s.Stats.IsDestroyed).ToList();
    }
    
    public void AddRound(CombatRound round)
    {
        Rounds.Add(round);
        CurrentRound++;
    }
}

public class CombatShip
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public ShipCombatStats Stats { get; set; } = new();
    public bool IsAttacker { get; set; }
    public int Initiative { get; set; }
    public CombatShip? CurrentTarget { get; set; }
    
    public void InitializeCombat()
    {
        Stats.InitializeCombat();
        Initiative = Stats.Speed + Random.Shared.Next(1, 11); // Speed + 1d10
    }
    
    public void UpdateInitiative()
    {
        Initiative = Stats.Speed + Random.Shared.Next(1, 11);
    }
}

public class CombatRound
{
    public int RoundNumber { get; set; }
    public List<CombatAction> Actions { get; set; } = new();
    public List<CombatShip> DestroyedShips { get; set; } = new();
    public bool CombatEnded { get; set; } = false;
    public string? EndReason { get; set; }
}

public class CombatAction
{
    public Guid ActorId { get; set; }
    public Guid? TargetId { get; set; }
    public ActionType Type { get; set; }
    public AttackResult? AttackResult { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum ActionType
{
    Attack,
    SpecialAbility,
    Retreat,
    Repair,
    NoAction
}

public class CombatResult
{
    public Guid CombatId { get; set; }
    public CombatType Type { get; set; }
    public bool AttackerVictory { get; set; }
    public int RoundsFought { get; set; }
    public TimeSpan Duration { get; set; }
    
    // Casualties
    public List<CombatShip> AttackerLosses { get; set; } = new();
    public List<CombatShip> DefenderLosses { get; set; } = new();
    
    // Rewards and consequences
    public List<CombatReward> Rewards { get; set; } = new();
    public List<CombatConsequence> Consequences { get; set; } = new();
    
    // Cinematic data for UI
    public CombatCinematicData CinematicData { get; set; } = new();
}

public class CombatReward
{
    public RewardType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public Guid? TargetId { get; set; }
}

public enum RewardType
{
    Experience,
    Resources,
    Technology,
    Intelligence,
    Reputation
}

public class CombatConsequence
{
    public ConsequenceType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }
    public Guid? TargetId { get; set; }
}

public enum ConsequenceType
{
    MoraleLoss,
    ResourceLoss,
    TechnologyLoss,
    ReputationLoss,
    StrategicDisadvantage
}

public class CombatCinematicData
{
    public List<string> Highlights { get; set; } = new();
    public List<string> CriticalMoments { get; set; } = new();
    public string FinalNarrative { get; set; } = string.Empty;
    public Dictionary<string, object> CustomData { get; set; } = new();
}

public class CombatEnvironment
{
    public TerrainType Terrain { get; set; } = TerrainType.Space;
    public WeatherCondition Weather { get; set; } = WeatherCondition.Clear;
    public double Visibility { get; set; } = 1.0;
    public double Gravity { get; set; } = 1.0;
    public bool HasAtmosphere { get; set; } = false;
    
    public double GetAccuracyModifier()
    {
        var modifier = 1.0;
        
        // Weather effects
        modifier *= Weather switch
        {
            WeatherCondition.Storm => 0.7,
            WeatherCondition.Fog => 0.8,
            WeatherCondition.Clear => 1.0,
            _ => 1.0
        };
        
        // Visibility effects
        modifier *= Visibility;
        
        return modifier;
    }
}

public enum TerrainType
{
    Space,
    Planetary,
    Urban,
    Forest,
    Desert,
    Mountain,
    Asteroid,
    Nebula,
    Trench
}

public enum WeatherCondition
{
    Clear,
    Storm,
    Fog,
    Radiation
}

public enum CombatType
{
    FleetCombat,
    PlanetaryCombat,
    DeathStarRun,
    Mission
} 