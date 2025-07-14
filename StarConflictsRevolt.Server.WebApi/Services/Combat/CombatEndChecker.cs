using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public class CombatEndChecker : ICombatEndChecker
{
    private readonly ILogger<CombatEndChecker> _logger;

    public CombatEndChecker(ILogger<CombatEndChecker> logger)
    {
        _logger = logger;
    }

    public bool CheckCombatEnd(CombatState state)
    {
        // Check if maximum rounds reached
        if (state.CurrentRound >= state.MaxRounds)
        {
            _logger.LogDebug("Combat ended due to maximum rounds reached: {CurrentRound}/{MaxRounds}",
                state.CurrentRound, state.MaxRounds);
            return true;
        }

        // Check if all ships of one side are destroyed
        var activeAttackerShips = state.AttackerShips.Where(s => !s.Stats.IsDestroyed).ToList();
        var activeDefenderShips = state.DefenderShips.Where(s => !s.Stats.IsDestroyed).ToList();

        if (activeAttackerShips.Count == 0)
        {
            _logger.LogDebug("Combat ended: All attacker ships destroyed");
            return true;
        }

        if (activeDefenderShips.Count == 0)
        {
            _logger.LogDebug("Combat ended: All defender ships destroyed");
            return true;
        }

        // Check for retreat conditions
        if (ShouldRetreat(activeAttackerShips, activeDefenderShips, state))
        {
            _logger.LogDebug("Combat ended: Retreat conditions met");
            return true;
        }

        // Check for stalemate conditions
        if (IsStalemate(activeAttackerShips, activeDefenderShips, state))
        {
            _logger.LogDebug("Combat ended: Stalemate conditions met");
            return true;
        }

        return false;
    }

    public string? GetEndReason(CombatState state)
    {
        // Check if maximum rounds reached
        if (state.CurrentRound >= state.MaxRounds) return "Maximum rounds reached";

        // Check if all ships of one side are destroyed
        var activeAttackerShips = state.AttackerShips.Where(s => !s.Stats.IsDestroyed).ToList();
        var activeDefenderShips = state.DefenderShips.Where(s => !s.Stats.IsDestroyed).ToList();

        if (activeAttackerShips.Count == 0) return "All attacker ships destroyed";

        if (activeDefenderShips.Count == 0) return "All defender ships destroyed";

        // Check for retreat conditions
        if (ShouldRetreat(activeAttackerShips, activeDefenderShips, state)) return "Retreat conditions met";

        // Check for stalemate conditions
        if (IsStalemate(activeAttackerShips, activeDefenderShips, state)) return "Stalemate conditions met";

        return null;
    }

    private bool ShouldRetreat(List<CombatShip> attackerShips, List<CombatShip> defenderShips, CombatState state)
    {
        // Calculate total combat effectiveness for each side
        var attackerEffectiveness = CalculateSideEffectiveness(attackerShips);
        var defenderEffectiveness = CalculateSideEffectiveness(defenderShips);

        // Check if one side is significantly outmatched
        var effectivenessRatio = attackerEffectiveness / defenderEffectiveness;

        if (effectivenessRatio < 0.2) // Attacker is severely outmatched
        {
            _logger.LogDebug("Attacker should retreat: Effectiveness ratio {Ratio}", effectivenessRatio);
            return true;
        }

        if (effectivenessRatio > 5.0) // Defender is severely outmatched
        {
            _logger.LogDebug("Defender should retreat: Effectiveness ratio {Ratio}", effectivenessRatio);
            return true;
        }

        // Check for low morale conditions (many ships heavily damaged)
        var attackerLowMorale = attackerShips.Count(s => s.Stats.GetCombatEffectiveness() < 0.3) > attackerShips.Count * 0.7;
        var defenderLowMorale = defenderShips.Count(s => s.Stats.GetCombatEffectiveness() < 0.3) > defenderShips.Count * 0.7;

        if (attackerLowMorale || defenderLowMorale)
        {
            _logger.LogDebug("Retreat due to low morale: Attacker={AttackerLow}, Defender={DefenderLow}",
                attackerLowMorale, defenderLowMorale);
            return true;
        }

        return false;
    }

    private bool IsStalemate(List<CombatShip> attackerShips, List<CombatShip> defenderShips, CombatState state)
    {
        // Check if combat has been going on for too long without significant progress
        if (state.CurrentRound < 10) return false; // Need at least 10 rounds to consider stalemate

        // Calculate damage dealt in recent rounds
        var recentRounds = state.Rounds.TakeLast(3).ToList();
        var totalDamageDealt = recentRounds.Sum(r => r.Actions.Count(a => a.AttackResult?.Hit == true));

        // If very little damage is being dealt, it's a stalemate
        if (totalDamageDealt < 2)
        {
            _logger.LogDebug("Stalemate detected: Low damage in recent rounds ({Damage})", totalDamageDealt);
            return true;
        }

        // Check if both sides are equally matched and no progress is being made
        var attackerEffectiveness = CalculateSideEffectiveness(attackerShips);
        var defenderEffectiveness = CalculateSideEffectiveness(defenderShips);
        var effectivenessRatio = attackerEffectiveness / defenderEffectiveness;

        if (effectivenessRatio > 0.8 && effectivenessRatio < 1.2)
        {
            // Check if this has been the case for several rounds
            var roundsWithSimilarEffectiveness = 0;
            for (var i = Math.Max(0, state.Rounds.Count - 5); i < state.Rounds.Count; i++)
                // This is a simplified check - in a real implementation, you'd track effectiveness over time
                roundsWithSimilarEffectiveness++;

            if (roundsWithSimilarEffectiveness >= 3)
            {
                _logger.LogDebug("Stalemate detected: Similar effectiveness for multiple rounds");
                return true;
            }
        }

        return false;
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