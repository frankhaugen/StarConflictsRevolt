using StarConflictsRevolt.Server.WebApi.Models.Combat;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public class TargetSelector : ITargetSelector
{
    private readonly ILogger<TargetSelector> _logger;

    public TargetSelector(ILogger<TargetSelector> logger)
    {
        _logger = logger;
    }

    public CombatShip? SelectTarget(CombatShip attacker, List<CombatShip> enemies, CombatState state)
    {
        if (enemies.Count == 0) return null;

        // If there's only one enemy, target it
        if (enemies.Count == 1) return enemies[0];

        // Calculate threat scores for each enemy
        var targetScores = enemies.Select(enemy => new TargetScore
        {
            Ship = enemy,
            Score = CalculateThreatScore(attacker, enemy, state)
        }).ToList();

        // Sort by threat score (highest first)
        targetScores.Sort((a, b) => b.Score.CompareTo(a.Score));

        // Select target based on strategy
        var selectedTarget = SelectTargetByStrategy(attacker, targetScores, state);

        _logger.LogDebug("Selected target {TargetId} for attacker {AttackerId} with score {Score}",
            selectedTarget?.Id, attacker.Id, targetScores.FirstOrDefault()?.Score);

        return selectedTarget;
    }

    private double CalculateThreatScore(CombatShip attacker, CombatShip enemy, CombatState state)
    {
        var baseScore = 0.0;

        // Damage potential (how much damage the enemy can do)
        var damagePotential = enemy.Stats.Attack * enemy.Stats.GetCombatEffectiveness();
        baseScore += damagePotential * 2.0;

        // Health remaining (prioritize damaged enemies)
        var healthPercentage = enemy.Stats.GetCombatEffectiveness();
        baseScore += (1.0 - healthPercentage) * 1.5; // Damaged enemies get higher priority

        // Range factor (closer enemies are easier to hit)
        var rangeModifier = CalculateRangeModifier(attacker, enemy);
        baseScore *= rangeModifier;

        // Special abilities threat
        var abilityThreat = CalculateAbilityThreat(enemy);
        baseScore += abilityThreat;

        // Environmental modifiers
        var environmentalModifier = state.Environment.GetAccuracyModifier();
        baseScore *= environmentalModifier;

        // Random factor to add unpredictability
        var randomFactor = 0.8 + (Random.Shared.NextDouble() * 0.4); // 0.8 to 1.2
        baseScore *= randomFactor;

        return baseScore;
    }

    private double CalculateRangeModifier(CombatShip attacker, CombatShip enemy)
    {
        // Simple range calculation - could be enhanced with actual positioning
        var maxRange = Math.Max(attacker.Stats.Range, enemy.Stats.Range);
        var effectiveRange = Math.Min(attacker.Stats.Range, maxRange);
        
        // Closer targets are easier to hit
        return 1.0 + (effectiveRange * 0.1);
    }

    private double CalculateAbilityThreat(CombatShip enemy)
    {
        var threat = 0.0;
        
        foreach (var ability in enemy.Stats.Abilities)
        {
            if (ability.CanActivate())
            {
                threat += ability.EffectValue;
            }
        }
        
        return threat;
    }

    private CombatShip? SelectTargetByStrategy(CombatShip attacker, List<TargetScore> targetScores, CombatState state)
    {
        // Simple strategy: target the highest threat enemy
        // This could be enhanced with different AI strategies
        
        if (targetScores.Count == 0) return null;
        
        // 80% chance to target highest threat, 20% chance to target random high-threat enemy
        if (Random.Shared.NextDouble() < 0.8)
        {
            return targetScores[0].Ship;
        }
        else
        {
            // Select from top 3 threats randomly
            var topThreats = targetScores.Take(Math.Min(3, targetScores.Count)).ToList();
            var randomIndex = Random.Shared.Next(topThreats.Count);
            return topThreats[randomIndex].Ship;
        }
    }
} 