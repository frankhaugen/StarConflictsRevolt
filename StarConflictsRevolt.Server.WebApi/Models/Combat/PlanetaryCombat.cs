using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

public class BombardmentResult
{
    public bool Success { get; set; }
    public int StructureDamage { get; set; }
    public int PopulationCasualties { get; set; }
    public int DefenseCasualties { get; set; }
    public List<Structure> DestroyedStructures { get; set; } = new();
    public bool ShieldGeneratorDestroyed { get; set; } = false;
    public string Description { get; set; } = string.Empty;
}

public class GroundCombat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AttackerId { get; set; }
    public Guid DefenderId { get; set; }
    public Planet Location { get; set; } = new("Combat Location", 0, 0, 0, 0, 0, new(), new(), null, 0, 0, 0, 0, 0, 0, 0, PlanetType.Terran, 0, 0, 0);
    
    // Ground forces
    public List<GroundUnit> AttackerUnits { get; set; } = new();
    public List<GroundUnit> DefenderUnits { get; set; } = new();
    
    // Combat state
    public GroundCombatState State { get; set; } = new();
    public List<GroundCombatRound> Rounds { get; set; } = new();
    public GroundCombatResult? Result { get; set; }
    
    public bool IsCombatEnded => Result != null || State.CurrentRound >= State.MaxRounds;
    
    public List<GroundUnit> GetAllUnits()
    {
        return AttackerUnits.Concat(DefenderUnits).ToList();
    }
    
    public List<GroundUnit> GetActiveUnits()
    {
        return GetAllUnits().Where(u => !u.IsDestroyed).ToList();
    }
}

public class GroundUnit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public UnitType Type { get; set; }
    public Guid OwnerId { get; set; }
    public bool IsAttacker { get; set; }
    
    // Combat stats
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Health { get; set; }
    public int CurrentHealth { get; set; }
    public int Speed { get; set; }
    public int Range { get; set; }
    public double Accuracy { get; set; }
    
    // Special abilities
    public List<GroundUnitAbility> Abilities { get; set; } = new();
    
    // Current state
    public bool IsDestroyed => CurrentHealth <= 0;
    public bool IsSuppressed { get; set; } = false;
    public int SuppressionRounds { get; set; } = 0;
    public GroundUnit? CurrentTarget { get; set; }
    
    public void InitializeCombat()
    {
        CurrentHealth = Health;
        IsSuppressed = false;
        SuppressionRounds = 0;
        CurrentTarget = null;
    }
    
    public void ApplyDamage(int damage)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - damage);
        
        // Check for suppression (damage > 50% of max health)
        if (damage > Health / 2 && !IsDestroyed)
        {
            IsSuppressed = true;
            SuppressionRounds = 2;
        }
    }
    
    public void UpdateSuppression()
    {
        if (IsSuppressed && SuppressionRounds > 0)
        {
            SuppressionRounds--;
            if (SuppressionRounds <= 0)
            {
                IsSuppressed = false;
            }
        }
    }
    
    public double GetCombatEffectiveness()
    {
        if (IsDestroyed) return 0.0;
        
        var healthEffectiveness = CurrentHealth / (double)Health;
        var suppressionPenalty = IsSuppressed ? 0.5 : 1.0;
        
        return healthEffectiveness * suppressionPenalty;
    }
}

public enum UnitType
{
    Infantry,
    HeavyInfantry,
    Artillery,
    Armor,
    AntiAir,
    SpecialForces,
    Militia,
    SecurityForces
}

public class GroundUnitAbility
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

public class GroundCombatState
{
    public int CurrentRound { get; set; } = 0;
    public int MaxRounds { get; set; } = 15;
    public TerrainType Terrain { get; set; } = TerrainType.Planetary;
    public WeatherCondition Weather { get; set; } = WeatherCondition.Clear;
    public bool IsNight { get; set; } = false;
    public bool HasCover { get; set; } = false;
    
    public double GetTerrainModifier()
    {
        return Terrain switch
        {
            TerrainType.Planetary => 1.0,
            TerrainType.Urban => 0.8, // Urban combat is more difficult
            TerrainType.Forest => 0.7, // Forest provides cover
            TerrainType.Desert => 1.2, // Open terrain
            TerrainType.Mountain => 0.6, // Mountainous terrain
            _ => 1.0
        };
    }
    
    public double GetWeatherModifier()
    {
        return Weather switch
        {
            WeatherCondition.Clear => 1.0,
            WeatherCondition.Storm => 0.6,
            WeatherCondition.Fog => 0.7,
            WeatherCondition.Radiation => 0.8,
            _ => 1.0
        };
    }
    
    public double GetVisibilityModifier()
    {
        var modifier = 1.0;
        
        if (IsNight) modifier *= 0.5;
        if (HasCover) modifier *= 0.8;
        
        return modifier * GetWeatherModifier();
    }
}

public class GroundCombatRound
{
    public int RoundNumber { get; set; }
    public List<GroundCombatAction> Actions { get; set; } = new();
    public List<GroundUnit> DestroyedUnits { get; set; } = new();
    public bool CombatEnded { get; set; } = false;
    public string? EndReason { get; set; }
}

public class GroundCombatAction
{
    public Guid ActorId { get; set; }
    public Guid? TargetId { get; set; }
    public GroundActionType Type { get; set; }
    public GroundAttackResult? AttackResult { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum GroundActionType
{
    Attack,
    SpecialAbility,
    Retreat,
    Suppress,
    NoAction
}

public class GroundAttackResult
{
    public bool Hit { get; set; }
    public int Damage { get; set; }
    public bool Critical { get; set; }
    public bool Suppressed { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public static GroundAttackResult Miss => new() { Hit = false, Description = "Attack missed" };
    
    public static GroundAttackResult CreateHit(int damage, bool critical = false, bool suppressed = false)
    {
        return new GroundAttackResult
        {
            Hit = true,
            Damage = damage,
            Critical = critical,
            Suppressed = suppressed,
            Description = critical ? "Critical hit!" : "Attack hit"
        };
    }
}

public class GroundCombatResult
{
    public bool AttackerVictory { get; set; }
    public int RoundsFought { get; set; }
    public TimeSpan Duration { get; set; }
    
    // Casualties
    public List<GroundUnit> AttackerLosses { get; set; } = new();
    public List<GroundUnit> DefenderLosses { get; set; } = new();
    
    // Planetary effects
    public int PopulationCasualties { get; set; }
    public int InfrastructureDamage { get; set; }
    public List<Structure> DestroyedStructures { get; set; } = new();
    
    // Capture result
    public CaptureResult CaptureResult { get; set; } = new();
    
    // Cinematic data
    public GroundCombatCinematicData CinematicData { get; set; } = new();
}

public class CaptureResult
{
    public bool PlanetCaptured { get; set; }
    public Guid? NewOwnerId { get; set; }
    public int ResistanceLevel { get; set; } = 0; // 0-100, affects future stability
    public int LoyaltyChange { get; set; } = 0; // -100 to +100
    public List<string> CaptureEvents { get; set; } = new();
    public string FinalNarrative { get; set; } = string.Empty;
}

public class GroundCombatCinematicData
{
    public List<string> Highlights { get; set; } = new();
    public List<string> CriticalMoments { get; set; } = new();
    public string FinalNarrative { get; set; } = string.Empty;
    public Dictionary<string, object> CustomData { get; set; } = new();
} 