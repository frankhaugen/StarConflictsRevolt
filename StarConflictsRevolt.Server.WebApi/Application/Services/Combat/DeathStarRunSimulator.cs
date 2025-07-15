using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public class DeathStarRunSimulator : IDeathStarRunSimulator
{
    private readonly ILogger<DeathStarRunSimulator> _logger;

    public DeathStarRunSimulator(ILogger<DeathStarRunSimulator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public CombatResult SimulateDeathStarRun(Fleet attacker, DeathStar defender)
    {
        _logger.LogInformation("Starting Death Star run simulation. Attacker: {AttackerId}, Death Star: {DeathStarId}",
            attacker.Id, defender.Id);

        var state = new DeathStarRunState
        {
            Phase = RunPhase.Approach,
            SurvivingShips = attacker.Ships.Count
        };

        defender.InitializeCombat();

        // Phase 1: Approach
        if (!ResolveApproachPhase(attacker, defender, state))
        {
            return new CombatResult
            {
                CombatId = Guid.NewGuid(),
                Type = CombatType.DeathStarRun,
                AttackerVictory = false,
                RoundsFought = 3,
                Duration = TimeSpan.FromSeconds(6),
                AttackerLosses = attacker.Ships.Select(s => new CombatShip { Id = s.Id, Name = s.Model }).ToList(),
                DefenderLosses = new List<CombatShip>()
            };
        }

        // Phase 2: Trench Entry
        if (!ResolveTrenchEntry(attacker, defender, state))
        {
            return new CombatResult
            {
                CombatId = Guid.NewGuid(),
                Type = CombatType.DeathStarRun,
                AttackerVictory = false,
                RoundsFought = 5,
                Duration = TimeSpan.FromSeconds(10),
                AttackerLosses = attacker.Ships.Select(s => new CombatShip { Id = s.Id, Name = s.Model }).ToList(),
                DefenderLosses = new List<CombatShip>()
            };
        }

        // Phase 3: Trench Run
        var trenchResult = ResolveTrenchRun(attacker, defender, state);
        if (trenchResult != TrenchRunResult.Success)
        {
            return new CombatResult
            {
                CombatId = Guid.NewGuid(),
                Type = CombatType.DeathStarRun,
                AttackerVictory = false,
                RoundsFought = 10,
                Duration = TimeSpan.FromSeconds(20),
                AttackerLosses = attacker.Ships.Select(s => new CombatShip { Id = s.Id, Name = s.Model }).ToList(),
                DefenderLosses = new List<CombatShip>()
            };
        }

        // Phase 4: Exhaust Port Attack
        return ResolveExhaustPortAttack(attacker, defender, state);
    }

    /// <inheritdoc />
    public bool ResolveApproachPhase(Fleet attacker, DeathStar defender, DeathStarRunState state)
    {
        _logger.LogDebug("Resolving Death Star approach phase");

        // Approach phase: 3 rounds of heavy turbolaser fire
        for (var round = 0; round < 3; round++)
        {
            // Generate turbolaser fire
            var turbolaserShots = defender.GenerateTurbolaserFire();
            state.TurbolaserFire.AddRange(turbolaserShots);
            
            // Apply damage to attacking ships (simplified)
            var shipsLost = Math.Min(turbolaserShots.Count / 3, state.SurvivingShips);
            state.SurvivingShips -= shipsLost;

            // Check if all ships are destroyed
            if (state.SurvivingShips <= 0)
            {
                _logger.LogInformation("All attacking ships destroyed during approach phase");
                return false;
            }
        }

        state.Phase = RunPhase.TrenchEntry;
        _logger.LogDebug("Approach phase completed. {ShipsRemaining} ships remaining", state.SurvivingShips);
        return true;
    }

    /// <inheritdoc />
    public bool ResolveTrenchEntry(Fleet attacker, DeathStar defender, DeathStarRunState state)
    {
        _logger.LogDebug("Resolving Death Star trench entry phase");

        // Trench entry: 2 rounds of TIE interceptor attacks
        for (var round = 0; round < 2; round++)
        {
            // TIE interceptor attacks (simplified)
            var tieAttacks = Math.Min(defender.ActiveTIEInterceptors, 20);
            var shipsLost = Math.Min(tieAttacks / 4, state.SurvivingShips);
            state.SurvivingShips -= shipsLost;

            // Check if all ships are destroyed
            if (state.SurvivingShips <= 0)
            {
                _logger.LogInformation("All attacking ships destroyed during trench entry");
                return false;
            }
        }

        state.Phase = RunPhase.TrenchRun;
        _logger.LogDebug("Trench entry completed. {ShipsRemaining} ships remaining", state.SurvivingShips);
        return true;
    }

    /// <inheritdoc />
    public TrenchRunResult ResolveTrenchRun(Fleet attacker, DeathStar defender, DeathStarRunState state)
    {
        _logger.LogDebug("Resolving Death Star trench run phase");

        // Trench run: 5 rounds of intense combat
        for (var round = 0; round < 5; round++)
        {
            // Mixed turbolaser and TIE attacks (simplified)
            var turbolaserShots = defender.GenerateTurbolaserFire();
            var tieAttacks = Math.Min(defender.ActiveTIEInterceptors / 2, 15);
            
            var shipsLost = Math.Min((turbolaserShots.Count + tieAttacks) / 5, state.SurvivingShips);
            state.SurvivingShips -= shipsLost;

            // Check if all ships are destroyed
            if (state.SurvivingShips <= 0)
            {
                _logger.LogInformation("All attacking ships destroyed during trench run");
                return TrenchRunResult.Destroyed;
            }
        }

        state.Phase = RunPhase.ExhaustPortAttack;
        _logger.LogDebug("Trench run completed. {ShipsRemaining} ships remaining", state.SurvivingShips);
        return TrenchRunResult.Success;
    }

    /// <inheritdoc />
    public CombatResult ResolveExhaustPortAttack(Fleet attacker, DeathStar defender, DeathStarRunState state)
    {
        _logger.LogDebug("Resolving Death Star exhaust port attack phase");

        // Check if shield generator is destroyed
        if (!defender.ShieldGeneratorDestroyed)
        {
            _logger.LogInformation("Shield generator still active - exhaust port attack failed");
            return new CombatResult
            {
                CombatId = Guid.NewGuid(),
                Type = CombatType.DeathStarRun,
                AttackerVictory = false,
                RoundsFought = 11,
                Duration = TimeSpan.FromSeconds(22),
                AttackerLosses = attacker.Ships.Select(s => new CombatShip { Id = s.Id, Name = s.Model }).ToList(),
                DefenderLosses = new List<CombatShip>()
            };
        }

        // Check if exhaust port is vulnerable
        if (!defender.ExhaustPortVulnerable)
        {
            _logger.LogInformation("Exhaust port not vulnerable - attack failed");
            return new CombatResult
            {
                CombatId = Guid.NewGuid(),
                Type = CombatType.DeathStarRun,
                AttackerVictory = false,
                RoundsFought = 11,
                Duration = TimeSpan.FromSeconds(22),
                AttackerLosses = attacker.Ships.Select(s => new CombatShip { Id = s.Id, Name = s.Model }).ToList(),
                DefenderLosses = new List<CombatShip>()
            };
        }

        // Final attack on exhaust port
        var attackSuccess = Random.Shared.NextDouble() < (0.15 * state.SurvivingShips); // 15% chance per surviving ship

        if (attackSuccess)
        {
            _logger.LogInformation("Death Star destroyed! Attack successful!");
            return new CombatResult
            {
                CombatId = Guid.NewGuid(),
                Type = CombatType.DeathStarRun,
                AttackerVictory = true,
                RoundsFought = 11,
                Duration = TimeSpan.FromSeconds(22),
                AttackerLosses = attacker.Ships.Select(s => new CombatShip { Id = s.Id, Name = s.Model }).ToList(),
                DefenderLosses = new List<CombatShip> { new CombatShip { Id = defender.Id, Name = defender.Name } }
            };
        }
        else
        {
            _logger.LogInformation("Exhaust port attack failed - all ships missed");
            return new CombatResult
            {
                CombatId = Guid.NewGuid(),
                Type = CombatType.DeathStarRun,
                AttackerVictory = false,
                RoundsFought = 11,
                Duration = TimeSpan.FromSeconds(22),
                AttackerLosses = attacker.Ships.Select(s => new CombatShip { Id = s.Id, Name = s.Model }).ToList(),
                DefenderLosses = new List<CombatShip>()
            };
        }
    }
}