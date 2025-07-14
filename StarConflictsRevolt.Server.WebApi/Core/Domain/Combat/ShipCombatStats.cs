namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class ShipCombatStats
{
    public int Attack { get; set; } // Base attack power
    public int Defense { get; set; } // Base defense rating
    public int Shields { get; set; } // Shield strength
    public int Hull { get; set; } // Hull integrity
    public int Speed { get; set; } // Initiative modifier
    public int Range { get; set; } // Weapon range
    public double Accuracy { get; set; } // Hit probability
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