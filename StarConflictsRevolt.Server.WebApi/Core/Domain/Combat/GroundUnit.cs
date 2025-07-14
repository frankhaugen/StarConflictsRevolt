namespace StarConflictsRevolt.Server.WebApi.Models.Combat;

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
    public bool IsSuppressed { get; set; }
    public int SuppressionRounds { get; set; }
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
            if (SuppressionRounds <= 0) IsSuppressed = false;
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