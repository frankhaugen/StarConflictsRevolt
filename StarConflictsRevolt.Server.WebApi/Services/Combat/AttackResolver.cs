using StarConflictsRevolt.Server.WebApi.Models.Combat;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public class AttackResolver : IAttackResolver
{
    private readonly ILogger<AttackResolver> _logger;

    public AttackResolver(ILogger<AttackResolver> logger)
    {
        _logger = logger;
    }

    public AttackResult ResolveAttack(CombatShip attacker, CombatShip target, CombatState state)
    {
        // Calculate hit chance
        var hitChance = CalculateHitChance(attacker, target, state);
        
        // Determine if attack hits
        var hit = Random.Shared.NextDouble() < hitChance;
        
        if (!hit)
        {
            return AttackResult.Miss;
        }

        // Calculate damage
        var damageModifiers = CalculateDamageModifiers(attacker, target, state);
        var baseDamage = attacker.Stats.Attack;
        var totalDamage = (int)(baseDamage * damageModifiers);

        // Determine critical hit
        var criticalChance = CalculateCriticalChance(attacker, target, state);
        var isCritical = Random.Shared.NextDouble() < criticalChance;
        
        if (isCritical)
        {
            totalDamage = (int)(totalDamage * 1.5); // 50% bonus for critical hits
        }

        // Split damage between shields and hull
        var (shieldDamage, hullDamage) = CalculateDamageSplit(totalDamage, target);

        _logger.LogDebug("Attack resolved: Attacker={AttackerId}, Target={TargetId}, Hit={Hit}, Damage={Damage}, Critical={Critical}",
            attacker.Id, target.Id, hit, totalDamage, isCritical);

        return AttackResult.CreateHit(shieldDamage, hullDamage, isCritical);
    }

    public double CalculateHitChance(CombatShip attacker, CombatShip target, CombatState state)
    {
        var baseAccuracy = attacker.Stats.Accuracy;
        
        // Defender evasion based on speed and effectiveness
        var evasion = target.Stats.Speed * 0.1 * target.Stats.GetCombatEffectiveness();
        
        // Environmental modifiers
        var environmentalModifier = state.Environment.GetAccuracyModifier();
        
        // Range penalty (simplified)
        var rangePenalty = CalculateRangePenalty(attacker, target);
        
        // Final hit chance calculation
        var hitChance = (baseAccuracy - evasion) * environmentalModifier * rangePenalty;
        
        // Clamp between 0.05 and 0.95
        return Math.Max(0.05, Math.Min(0.95, hitChance));
    }

    public double CalculateDamageModifiers(CombatShip attacker, CombatShip target, CombatState state)
    {
        var modifier = 1.0;

        // Attacker effectiveness
        modifier *= attacker.Stats.GetCombatEffectiveness();

        // Target defense
        var defenseReduction = target.Stats.Defense * 0.1;
        modifier *= Math.Max(0.1, 1.0 - defenseReduction);

        // Environmental factors
        modifier *= state.Environment.GetAccuracyModifier();

        // Special abilities
        modifier *= CalculateAbilityModifiers(attacker, target);

        return modifier;
    }

    private double CalculateRangePenalty(CombatShip attacker, CombatShip target)
    {
        // Simple range penalty - could be enhanced with actual positioning
        var maxRange = Math.Max(attacker.Stats.Range, target.Stats.Range);
        var effectiveRange = Math.Min(attacker.Stats.Range, maxRange);
        
        // Penalty increases with range
        return Math.Max(0.5, 1.0 - (effectiveRange * 0.1));
    }

    private double CalculateCriticalChance(CombatShip attacker, CombatShip target, CombatState state)
    {
        var baseCriticalChance = 0.05; // 5% base chance
        
        // Attacker accuracy bonus
        var accuracyBonus = (attacker.Stats.Accuracy - 0.5) * 0.1;
        
        // Target size penalty (larger targets are easier to hit critically)
        var sizePenalty = target.Stats.Hull > 50 ? 0.02 : 0.0;
        
        // Environmental bonus
        var environmentalBonus = state.Environment.Visibility > 0.8 ? 0.01 : 0.0;
        
        var totalChance = baseCriticalChance + accuracyBonus + sizePenalty + environmentalBonus;
        
        return Math.Max(0.01, Math.Min(0.25, totalChance)); // Clamp between 1% and 25%
    }

    private (int shieldDamage, int hullDamage) CalculateDamageSplit(int totalDamage, CombatShip target)
    {
        var shieldDamage = 0;
        var hullDamage = 0;

        if (target.Stats.CurrentShields > 0)
        {
            // 70% of damage goes to shields first
            shieldDamage = Math.Min(target.Stats.CurrentShields, (int)(totalDamage * 0.7));
            hullDamage = totalDamage - shieldDamage;
        }
        else
        {
            // All damage goes to hull if no shields
            hullDamage = totalDamage;
        }

        return (shieldDamage, hullDamage);
    }

    private double CalculateAbilityModifiers(CombatShip attacker, CombatShip target)
    {
        var modifier = 1.0;

        // Check attacker abilities
        foreach (var ability in attacker.Stats.Abilities)
        {
            if (ability.IsActive)
            {
                switch (ability.Type)
                {
                    case AbilityType.IonCannon:
                        modifier *= 1.2; // 20% bonus against shields
                        break;
                    case AbilityType.ProtonTorpedo:
                        modifier *= 1.5; // 50% bonus damage
                        break;
                    case AbilityType.Stealth:
                        modifier *= 1.1; // 10% accuracy bonus
                        break;
                }
            }
        }

        // Check target abilities
        foreach (var ability in target.Stats.Abilities)
        {
            if (ability.IsActive)
            {
                switch (ability.Type)
                {
                    case AbilityType.Stealth:
                        modifier *= 0.8; // 20% damage reduction
                        break;
                }
            }
        }

        return modifier;
    }
} 