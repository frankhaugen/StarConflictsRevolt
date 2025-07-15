using StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Combat;

public interface IDeathStarRunCombatService
{
    DeathStarRunResult ExecuteDeathStarRun(Fleet attackingFleet, DeathStar deathStar, List<Character> heroes);
    TrenchRunResult ExecuteTrenchRun(Fleet fleet, DeathStar deathStar, List<Character> heroes);
    bool ExecuteShieldGeneratorAttack(Fleet fleet, DeathStar deathStar, Character hero);
    bool ExecuteExhaustPortAttack(Fleet fleet, DeathStar deathStar, Character hero);
    List<TurbolaserShot> GenerateTurbolaserDefense(DeathStar deathStar, int round);
    List<TIEFighter> GenerateTIEInterceptors(DeathStar deathStar, int round);
}

public class DeathStarRunCombatService : IDeathStarRunCombatService
{
    private readonly ILogger<DeathStarRunCombatService> _logger;

    public DeathStarRunCombatService(ILogger<DeathStarRunCombatService> logger)
    {
        _logger = logger;
    }

    public DeathStarRunResult ExecuteDeathStarRun(Fleet attackingFleet, DeathStar deathStar, List<Character> heroes)
    {
        _logger.LogInformation("Starting Death Star Run with {ShipCount} ships and {HeroCount} heroes", 
            attackingFleet.Ships.Count, heroes.Count);

        var result = new DeathStarRunResult
        {
            RunId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            AttackingFleet = attackingFleet,
            DeathStar = deathStar,
            Heroes = heroes,
            Phase = DeathStarRunPhase.Approach
        };

        // Phase 1: Approach - Heavy turbolaser fire
        if (!ExecuteApproachPhase(result))
        {
            result.Phase = DeathStarRunPhase.Destroyed;
            result.EndTime = DateTime.UtcNow;
            result.Success = false;
            return result;
        }

        // Phase 2: Trench Entry - TIE Fighter interception
        if (!ExecuteTrenchEntryPhase(result))
        {
            result.Phase = DeathStarRunPhase.Destroyed;
            result.EndTime = DateTime.UtcNow;
            result.Success = false;
            return result;
        }

        // Phase 3: Trench Run - Intense combat
        var trenchResult = ExecuteTrenchRun(attackingFleet, deathStar, heroes);
        if (trenchResult != TrenchRunResult.Success)
        {
            result.Phase = DeathStarRunPhase.Destroyed;
            result.EndTime = DateTime.UtcNow;
            result.Success = false;
            return result;
        }

        // Phase 4: Shield Generator Attack
        if (!ExecuteShieldGeneratorAttack(attackingFleet, deathStar, heroes.FirstOrDefault()))
        {
            result.Phase = DeathStarRunPhase.ShieldGeneratorIntact;
            result.EndTime = DateTime.UtcNow;
            result.Success = false;
            return result;
        }

        // Phase 5: Exhaust Port Attack
        if (!ExecuteExhaustPortAttack(attackingFleet, deathStar, heroes.FirstOrDefault()))
        {
            result.Phase = DeathStarRunPhase.ExhaustPortMissed;
            result.EndTime = DateTime.UtcNow;
            result.Success = false;
            return result;
        }

        // Success!
        result.Phase = DeathStarRunPhase.Success;
        result.EndTime = DateTime.UtcNow;
        result.Success = true;
        result.DeathStarDestroyed = true;

        _logger.LogInformation("Death Star Run completed successfully! Death Star destroyed!");
        return result;
    }

    private bool ExecuteApproachPhase(DeathStarRunResult result)
    {
        _logger.LogDebug("Executing approach phase");

        // 3 rounds of heavy turbolaser fire
        for (int round = 1; round <= 3; round++)
        {
            var turbolaserShots = GenerateTurbolaserDefense(result.DeathStar, round);
            result.TurbolaserShots.AddRange(turbolaserShots);

            // Calculate damage to attacking fleet
            var damageDealt = turbolaserShots.Sum(shot => shot.Damage);
            var shipsLost = Math.Min(damageDealt / 50, result.AttackingFleet.Ships.Count); // 50 damage per ship

            // Remove destroyed ships
            for (int i = 0; i < shipsLost && result.AttackingFleet.Ships.Count > 0; i++)
            {
                result.AttackingFleet.Ships.RemoveAt(0);
            }

            result.ShipsLostInApproach += shipsLost;

            if (result.AttackingFleet.Ships.Count == 0)
            {
                _logger.LogInformation("All ships destroyed during approach phase");
                return false;
            }
        }

        result.Phase = DeathStarRunPhase.TrenchEntry;
        return true;
    }

    private bool ExecuteTrenchEntryPhase(DeathStarRunResult result)
    {
        _logger.LogDebug("Executing trench entry phase");

        // 2 rounds of TIE Fighter interception
        for (int round = 1; round <= 2; round++)
        {
            var tieFighters = GenerateTIEInterceptors(result.DeathStar, round);
            result.TIEFighters.AddRange(tieFighters);

            // Calculate interception effectiveness
            var tieCount = tieFighters.Count;
            var shipsLost = Math.Min(tieCount / 3, result.AttackingFleet.Ships.Count); // 3 TIE fighters per ship

            // Remove destroyed ships
            for (int i = 0; i < shipsLost && result.AttackingFleet.Ships.Count > 0; i++)
            {
                result.AttackingFleet.Ships.RemoveAt(0);
            }

            result.ShipsLostInTrenchEntry += shipsLost;

            if (result.AttackingFleet.Ships.Count == 0)
            {
                _logger.LogInformation("All ships destroyed during trench entry");
                return false;
            }
        }

        result.Phase = DeathStarRunPhase.TrenchRun;
        return true;
    }

    public TrenchRunResult ExecuteTrenchRun(Fleet fleet, DeathStar deathStar, List<Character> heroes)
    {
        _logger.LogDebug("Executing trench run phase");

        var hero = heroes.FirstOrDefault();
        var heroBonus = hero?.IsForceSensitive == true ? 0.2 : 0.0; // 20% bonus for Force-sensitive heroes

        // 5 rounds of intense trench combat
        for (int round = 1; round <= 5; round++)
        {
            // Mixed turbolaser and TIE attacks
            var turbolaserShots = GenerateTurbolaserDefense(deathStar, round + 3);
            var tieFighters = GenerateTIEInterceptors(deathStar, round + 2);

            var totalThreat = turbolaserShots.Count + tieFighters.Count;
            var survivalChance = Math.Max(0.1, 0.6 - (totalThreat * 0.05) + heroBonus); // Base 60% survival, reduced by threat

            if (Random.Shared.NextDouble() > survivalChance)
            {
                _logger.LogInformation("Fleet destroyed during trench run at round {Round}", round);
                return TrenchRunResult.Destroyed;
            }
        }

        return TrenchRunResult.Success;
    }

    public bool ExecuteShieldGeneratorAttack(Fleet fleet, DeathStar deathStar, Character hero)
    {
        _logger.LogDebug("Executing shield generator attack");

        if (deathStar.ShieldGeneratorDestroyed)
        {
            _logger.LogDebug("Shield generator already destroyed");
            return true;
        }

        // Calculate attack success based on hero skills and fleet size
        var heroBonus = hero?.Combat * 0.02 ?? 0.0; // 2% per combat skill point
        var fleetBonus = Math.Min(0.3, fleet.Ships.Count * 0.05); // 5% per ship, max 30%
        var baseChance = 0.4; // 40% base chance

        var successChance = Math.Min(0.95, baseChance + heroBonus + fleetBonus);

        if (Random.Shared.NextDouble() < successChance)
        {
            deathStar.ShieldGeneratorDestroyed = true;
            deathStar.ActiveTurbolaserBatteries /= 2; // Reduce turbolaser effectiveness
            _logger.LogInformation("Shield generator destroyed!");
            return true;
        }

        _logger.LogInformation("Shield generator attack failed");
        return false;
    }

    public bool ExecuteExhaustPortAttack(Fleet fleet, DeathStar deathStar, Character hero)
    {
        _logger.LogDebug("Executing exhaust port attack");

        if (!deathStar.ShieldGeneratorDestroyed)
        {
            _logger.LogDebug("Cannot attack exhaust port - shield generator still active");
            return false;
        }

        // Calculate attack success based on hero skills and timing
        var heroBonus = hero?.IsForceSensitive == true ? 0.3 : 0.0; // 30% bonus for Force-sensitive heroes
        var timingBonus = Random.Shared.NextDouble() * 0.2; // Random timing bonus (0-20%)
        var baseChance = 0.15; // 15% base chance (very difficult!)

        var successChance = Math.Min(0.95, baseChance + heroBonus + timingBonus);

        if (Random.Shared.NextDouble() < successChance)
        {
            _logger.LogInformation("Exhaust port attack successful! Death Star destroyed!");
            return true;
        }

        _logger.LogInformation("Exhaust port attack failed");
        return false;
    }

    public List<TurbolaserShot> GenerateTurbolaserDefense(DeathStar deathStar, int round)
    {
        var shots = new List<TurbolaserShot>();
        var activeBatteries = deathStar.ShieldGeneratorDestroyed ? 
            deathStar.ActiveTurbolaserBatteries / 2 : deathStar.ActiveTurbolaserBatteries;

        // Increase intensity in later rounds
        var fireRate = 0.3 + (round * 0.1); // 30% base + 10% per round
        fireRate = Math.Min(0.8, fireRate); // Cap at 80%

        for (int i = 0; i < activeBatteries; i++)
        {
            if (Random.Shared.NextDouble() < fireRate)
            {
                shots.Add(new TurbolaserShot
                {
                    Accuracy = 0.7 + (round * 0.05), // Improve accuracy over time
                    Damage = 50 + (round * 10), // Increase damage over time
                    BatteryId = i
                });
            }
        }

        return shots;
    }

    public List<TIEFighter> GenerateTIEInterceptors(DeathStar deathStar, int round)
    {
        var fighters = new List<TIEFighter>();
        var activeInterceptors = deathStar.ActiveTIEInterceptors;

        // Increase deployment in later rounds
        var deploymentRate = 0.4 + (round * 0.15); // 40% base + 15% per round
        deploymentRate = Math.Min(0.9, deploymentRate); // Cap at 90%

        var fightersToDeploy = (int)(activeInterceptors * deploymentRate);

        for (int i = 0; i < fightersToDeploy; i++)
        {
            fighters.Add(new TIEFighter
            {
                Id = Guid.NewGuid(),
                Accuracy = 0.6 + (round * 0.03), // Improve accuracy over time
                Damage = 25 + (round * 5) // Increase damage over time
            });
        }

        return fighters;
    }
}

public class DeathStarRunResult
{
    public Guid RunId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Fleet AttackingFleet { get; set; } = null!;
    public DeathStar DeathStar { get; set; } = null!;
    public List<Character> Heroes { get; set; } = new();
    public DeathStarRunPhase Phase { get; set; }
    public bool Success { get; set; }
    public bool DeathStarDestroyed { get; set; }
    public int ShipsLostInApproach { get; set; }
    public int ShipsLostInTrenchEntry { get; set; }
    public int ShipsLostInTrenchRun { get; set; }
    public List<TurbolaserShot> TurbolaserShots { get; set; } = new();
    public List<TIEFighter> TIEFighters { get; set; } = new();
    public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
}

public enum DeathStarRunPhase
{
    Approach,
    TrenchEntry,
    TrenchRun,
    ShieldGeneratorAttack,
    ExhaustPortAttack,
    Success,
    ShieldGeneratorIntact,
    ExhaustPortMissed,
    Destroyed
} 