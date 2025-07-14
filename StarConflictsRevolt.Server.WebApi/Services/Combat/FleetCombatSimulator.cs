using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Server.WebApi.Services.Combat;

public class FleetCombatSimulator : IFleetCombatSimulator
{
    private readonly ITargetSelector _targetSelector;
    private readonly IAttackResolver _attackResolver;
    private readonly ICombatEndChecker _combatEndChecker;
    private readonly ICombatResultCalculator _resultCalculator;
    private readonly ILogger<FleetCombatSimulator> _logger;

    public FleetCombatSimulator(
        ITargetSelector targetSelector,
        IAttackResolver attackResolver,
        ICombatEndChecker combatEndChecker,
        ICombatResultCalculator resultCalculator,
        ILogger<FleetCombatSimulator> logger)
    {
        _targetSelector = targetSelector;
        _attackResolver = attackResolver;
        _combatEndChecker = combatEndChecker;
        _resultCalculator = resultCalculator;
        _logger = logger;
    }

    public CombatResult SimulateFleetCombat(Fleet attacker, Fleet defender, Planet? location = null)
    {
        _logger.LogInformation("Initializing fleet combat between {AttackerId} and {DefenderId}", attacker.Id, defender.Id);

        var state = InitializeCombat(attacker, defender, location);
        var startTime = DateTime.UtcNow;

        while (!state.IsCombatEnded)
        {
            var round = SimulateCombatRound(state);
            state.AddRound(round);

            if (round.CombatEnded)
            {
                state.FinalResult = FinalizeCombat(state);
                break;
            }
        }

        if (state.FinalResult == null)
        {
            state.FinalResult = FinalizeCombat(state);
        }

        state.FinalResult.Duration = DateTime.UtcNow - startTime;
        return state.FinalResult;
    }

    public CombatState InitializeCombat(Fleet attacker, Fleet defender, Planet? location = null)
    {
        var state = new CombatState
        {
            Type = CombatType.FleetCombat,
            Location = location,
            Environment = CreateCombatEnvironment(location)
        };

        // Convert fleets to combat ships
        state.AttackerShips = ConvertFleetToCombatShips(attacker, true);
        state.DefenderShips = ConvertFleetToCombatShips(defender, false);

        // Initialize all ships
        foreach (var ship in state.GetAllShips())
        {
            ship.InitializeCombat();
        }

        _logger.LogInformation("Combat initialized with {AttackerShips} attacker ships and {DefenderShips} defender ships",
            state.AttackerShips.Count, state.DefenderShips.Count);

        return state;
    }

    public List<CombatShip> ConvertFleetToCombatShips(Fleet fleet, bool isAttacker)
    {
        var combatShips = new List<CombatShip>();

        foreach (var ship in fleet.Ships)
        {
            var combatShip = new CombatShip
            {
                Id = ship.Id,
                Name = ship.Model,
                OwnerId = fleet.OwnerId, // Use fleet owner ID since ship doesn't have one
                IsAttacker = isAttacker,
                Stats = new ShipCombatStats
                {
                    Attack = ship.AttackPower,
                    Defense = ship.DefensePower,
                    Shields = ship.MaxHealth / 2, // Estimate shields as half of max health
                    Hull = ship.MaxHealth,
                    Speed = (int)ship.Speed,
                    Range = 1, // Default range
                    Accuracy = 0.8, // Default accuracy
                    Abilities = new List<SpecialAbility>() // No abilities in current ship model
                }
            };

            combatShips.Add(combatShip);
        }

        return combatShips;
    }

    public List<CombatShip> DetermineInitiativeOrder(List<CombatShip> ships)
    {
        // Update initiative for all ships
        foreach (var ship in ships)
        {
            ship.UpdateInitiative();
        }

        // Sort by initiative (highest first)
        return ships.OrderByDescending(s => s.Initiative).ToList();
    }

    public CombatShip? SelectTarget(CombatShip attacker, List<CombatShip> enemies, CombatState state)
    {
        return _targetSelector.SelectTarget(attacker, enemies, state);
    }

    public AttackResult ResolveAttack(CombatShip attacker, CombatShip target, CombatState state)
    {
        return _attackResolver.ResolveAttack(attacker, target, state);
    }

    public void ApplyDamage(AttackResult attackResult, CombatShip target)
    {
        if (attackResult.Hit)
        {
            target.Stats.ApplyDamage(attackResult.ShieldDamage, attackResult.HullDamage);
            
            _logger.LogDebug("Applied damage to ship {ShipId}: Shield={ShieldDamage}, Hull={HullDamage}",
                target.Id, attackResult.ShieldDamage, attackResult.HullDamage);
        }
    }

    public bool CheckCombatEnd(CombatState state)
    {
        return _combatEndChecker.CheckCombatEnd(state);
    }

    public CombatResult FinalizeCombat(CombatState state)
    {
        return _resultCalculator.CalculateResult(state);
    }

    private CombatRound SimulateCombatRound(CombatState state)
    {
        var round = new CombatRound
        {
            RoundNumber = state.CurrentRound + 1
        };

        var activeShips = state.GetActiveShips();
        if (activeShips.Count == 0)
        {
            round.CombatEnded = true;
            round.EndReason = "No active ships remaining";
            return round;
        }

        // Determine initiative order
        var initiativeOrder = DetermineInitiativeOrder(activeShips);

        // Process each ship's action
        foreach (var ship in initiativeOrder)
        {
            if (ship.Stats.IsDestroyed) continue;

            var action = ProcessShipAction(ship, state);
            round.Actions.Add(action);

            // Check if ship was destroyed
            if (ship.Stats.IsDestroyed)
            {
                round.DestroyedShips.Add(ship);
            }
        }

        // Check if combat should end
        if (CheckCombatEnd(state))
        {
            round.CombatEnded = true;
            round.EndReason = _combatEndChecker.GetEndReason(state);
        }

        return round;
    }

    private CombatAction ProcessShipAction(CombatShip ship, CombatState state)
    {
        var enemies = ship.IsAttacker ? state.DefenderShips : state.AttackerShips;
        var activeEnemies = enemies.Where(e => !e.Stats.IsDestroyed).ToList();

        if (activeEnemies.Count == 0)
        {
            return new CombatAction
            {
                ActorId = ship.Id,
                Type = ActionType.NoAction,
                Description = "No enemies to attack"
            };
        }

        // Select target
        var target = SelectTarget(ship, activeEnemies, state);
        if (target == null)
        {
            return new CombatAction
            {
                ActorId = ship.Id,
                Type = ActionType.NoAction,
                Description = "No suitable target found"
            };
        }

        // Resolve attack
        var attackResult = ResolveAttack(ship, target, state);
        ApplyDamage(attackResult, target);

        return new CombatAction
        {
            ActorId = ship.Id,
            TargetId = target.Id,
            Type = ActionType.Attack,
            AttackResult = attackResult,
            Description = attackResult.Description
        };
    }

    private CombatEnvironment CreateCombatEnvironment(Planet? location)
    {
        if (location == null)
        {
            return new CombatEnvironment
            {
                Terrain = TerrainType.Space,
                Weather = WeatherCondition.Clear,
                Visibility = 1.0,
                Gravity = 0.0,
                HasAtmosphere = false
            };
        }

        return new CombatEnvironment
        {
            Terrain = TerrainType.Planetary,
            Weather = WeatherCondition.Clear, // Could be randomized based on planet type
            Visibility = 1.0,
            Gravity = 1.0,
            HasAtmosphere = true
        };
    }
} 