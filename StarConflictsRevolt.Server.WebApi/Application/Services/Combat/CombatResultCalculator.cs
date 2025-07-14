using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public class CombatResultCalculator : ICombatResultCalculator
{
    private readonly ILogger<CombatResultCalculator> _logger;

    public CombatResultCalculator(ILogger<CombatResultCalculator> logger)
    {
        _logger = logger;
    }

    public CombatResult CalculateResult(CombatState state)
    {
        var result = new CombatResult
        {
            CombatId = state.CombatId,
            Type = state.Type,
            RoundsFought = state.CurrentRound,
            Duration = DateTime.UtcNow - state.StartTime
        };

        // Determine victory
        var activeAttackerShips = state.AttackerShips.Where(s => !s.Stats.IsDestroyed).ToList();
        var activeDefenderShips = state.DefenderShips.Where(s => !s.Stats.IsDestroyed).ToList();

        if (activeAttackerShips.Count == 0 && activeDefenderShips.Count == 0)
        {
            // Mutual destruction - defender wins by default
            result.AttackerVictory = false;
        }
        else if (activeDefenderShips.Count == 0)
        {
            result.AttackerVictory = true;
        }
        else if (activeAttackerShips.Count == 0)
        {
            result.AttackerVictory = false;
        }
        else
        {
            // Calculate effectiveness-based victory
            var attackerEffectiveness = CalculateSideEffectiveness(activeAttackerShips);
            var defenderEffectiveness = CalculateSideEffectiveness(activeDefenderShips);
            result.AttackerVictory = attackerEffectiveness > defenderEffectiveness;
        }

        // Calculate casualties
        result.AttackerLosses = state.AttackerShips.Where(s => s.Stats.IsDestroyed).ToList();
        result.DefenderLosses = state.DefenderShips.Where(s => s.Stats.IsDestroyed).ToList();

        // Calculate rewards and consequences
        result.Rewards = CalculateRewards(state);
        result.Consequences = CalculateConsequences(state);

        // Generate cinematic data
        result.CinematicData = GenerateCinematicData(state);

        _logger.LogInformation("Combat result calculated: AttackerVictory={AttackerVictory}, Rounds={Rounds}, AttackerLosses={AttackerLosses}, DefenderLosses={DefenderLosses}",
            result.AttackerVictory, result.RoundsFought, result.AttackerLosses.Count, result.DefenderLosses.Count);

        return result;
    }

    public List<CombatReward> CalculateRewards(CombatState state)
    {
        var rewards = new List<CombatReward>();

        // Base experience for participating
        var baseExperience = state.CurrentRound * 10;
        rewards.Add(new CombatReward
        {
            Type = RewardType.Experience,
            Description = $"Combat experience from {state.CurrentRound} rounds of fighting",
            Value = baseExperience
        });

        // Victory bonus
        var activeAttackerShips = state.AttackerShips.Where(s => !s.Stats.IsDestroyed).ToList();
        var activeDefenderShips = state.DefenderShips.Where(s => !s.Stats.IsDestroyed).ToList();

        if (activeDefenderShips.Count == 0 && activeAttackerShips.Count > 0)
        {
            // Attacker victory
            var victoryBonus = 100 + state.DefenderShips.Count * 25;
            rewards.Add(new CombatReward
            {
                Type = RewardType.Experience,
                Description = "Victory bonus for destroying all enemy ships",
                Value = victoryBonus
            });

            // Resource rewards for victory
            rewards.Add(new CombatReward
            {
                Type = RewardType.Resources,
                Description = "Salvaged resources from destroyed enemy ships",
                Value = state.DefenderShips.Count(s => s.Stats.IsDestroyed) * 50
            });
        }
        else if (activeAttackerShips.Count == 0 && activeDefenderShips.Count > 0)
        {
            // Defender victory
            var victoryBonus = 100 + state.AttackerShips.Count * 25;
            rewards.Add(new CombatReward
            {
                Type = RewardType.Experience,
                Description = "Victory bonus for successful defense",
                Value = victoryBonus
            });
        }

        // Special rewards for exceptional performance
        var criticalHits = state.Rounds.Sum(r => r.Actions.Count(a => a.AttackResult?.Critical == true));
        if (criticalHits > 0)
            rewards.Add(new CombatReward
            {
                Type = RewardType.Experience,
                Description = $"Bonus for {criticalHits} critical hits",
                Value = criticalHits * 5
            });

        return rewards;
    }

    public List<CombatConsequence> CalculateConsequences(CombatState state)
    {
        var consequences = new List<CombatConsequence>();

        // Morale loss for heavy casualties
        var totalLosses = state.AttackerShips.Count(s => s.Stats.IsDestroyed) + state.DefenderShips.Count(s => s.Stats.IsDestroyed);
        if (totalLosses > 5)
            consequences.Add(new CombatConsequence
            {
                Type = ConsequenceType.MoraleLoss,
                Description = $"Heavy casualties ({totalLosses} ships lost) have affected morale",
                Value = totalLosses * 5
            });

        // Resource loss for destroyed ships
        var resourceLoss = totalLosses * 25;
        if (resourceLoss > 0)
            consequences.Add(new CombatConsequence
            {
                Type = ConsequenceType.ResourceLoss,
                Description = "Resources lost with destroyed ships",
                Value = resourceLoss
            });

        // Strategic consequences for complete destruction
        if (state.AttackerShips.All(s => s.Stats.IsDestroyed))
            consequences.Add(new CombatConsequence
            {
                Type = ConsequenceType.StrategicDisadvantage,
                Description = "Complete fleet destruction has strategic implications",
                Value = 50
            });

        if (state.DefenderShips.All(s => s.Stats.IsDestroyed))
            consequences.Add(new CombatConsequence
            {
                Type = ConsequenceType.StrategicDisadvantage,
                Description = "Defender's complete fleet destruction",
                Value = 50
            });

        return consequences;
    }

    public CombatCinematicData GenerateCinematicData(CombatState state)
    {
        var cinematicData = new CombatCinematicData();

        // Generate highlights from combat rounds
        foreach (var round in state.Rounds)
        {
            var criticalActions = round.Actions.Where(a => a.AttackResult?.Critical == true).ToList();
            foreach (var action in criticalActions)
            {
                var attacker = state.GetAllShips().FirstOrDefault(s => s.Id == action.ActorId);
                var target = action.TargetId.HasValue ? state.GetAllShips().FirstOrDefault(s => s.Id == action.TargetId.Value) : null;

                if (attacker != null && target != null) cinematicData.Highlights.Add($"Round {round.RoundNumber}: {attacker.Name} scores a critical hit on {target.Name}!");
            }

            // Check for ship destructions
            foreach (var destroyedShip in round.DestroyedShips) cinematicData.CriticalMoments.Add($"Round {round.RoundNumber}: {destroyedShip.Name} is destroyed!");
        }

        // Generate final narrative
        var activeAttackerShips = state.AttackerShips.Where(s => !s.Stats.IsDestroyed).ToList();
        var activeDefenderShips = state.DefenderShips.Where(s => !s.Stats.IsDestroyed).ToList();

        if (activeDefenderShips.Count == 0 && activeAttackerShips.Count > 0)
            cinematicData.FinalNarrative = $"The attackers have achieved a decisive victory, destroying all enemy ships while preserving {activeAttackerShips.Count} of their own vessels.";
        else if (activeAttackerShips.Count == 0 && activeDefenderShips.Count > 0)
            cinematicData.FinalNarrative = $"The defenders have successfully repelled the attack, destroying all enemy ships while maintaining {activeDefenderShips.Count} vessels.";
        else if (activeAttackerShips.Count == 0 && activeDefenderShips.Count == 0)
            cinematicData.FinalNarrative = "The battle has ended in mutual destruction, with no ships remaining on either side.";
        else
            cinematicData.FinalNarrative = $"The battle has ended with {activeAttackerShips.Count} attacker ships and {activeDefenderShips.Count} defender ships remaining.";

        // Add custom data
        cinematicData.CustomData["TotalRounds"] = state.CurrentRound;
        cinematicData.CustomData["TotalActions"] = state.Rounds.Sum(r => r.Actions.Count);
        cinematicData.CustomData["CriticalHits"] = state.Rounds.Sum(r => r.Actions.Count(a => a.AttackResult?.Critical == true));
        cinematicData.CustomData["TotalDamage"] = state.Rounds.Sum(r => r.Actions.Sum(a => a.AttackResult?.ShieldDamage + a.AttackResult?.HullDamage ?? 0));

        return cinematicData;
    }

    private double CalculateSideEffectiveness(List<CombatShip> ships)
    {
        if (ships.Count == 0) return 0.0;

        var totalEffectiveness = ships.Sum(s => s.Stats.GetCombatEffectiveness());
        var averageEffectiveness = totalEffectiveness / ships.Count;

        // Factor in the number of ships (more ships = more effectiveness)
        return averageEffectiveness * Math.Sqrt(ships.Count);
    }
}