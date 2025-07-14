using StarConflictsRevolt.Server.WebApi.Models.Combat;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class CombatSimulatorTests
{
    [Test]
    public async Task ShipCombatStats_ApplyDamage_ReducesShieldsAndHull()
    {
        // Arrange
        var stats = new ShipCombatStats
        {
            Shields = 100,
            Hull = 200,
            CurrentShields = 100,
            CurrentHull = 200
        };

        // Act
        stats.ApplyDamage(50, 30);

        // Assert
        await Assert.That(stats.CurrentShields).IsEqualTo(50);
        await Assert.That(stats.CurrentHull).IsEqualTo(170);
    }

    [Test]
    public async Task ShipCombatStats_GetCombatEffectiveness_ReturnsValidValue()
    {
        // Arrange
        var stats = new ShipCombatStats
        {
            Shields = 100,
            Hull = 200,
            CurrentShields = 50,
            CurrentHull = 100
        };

        // Act
        var effectiveness = stats.GetCombatEffectiveness();

        // Assert
        await Assert.That(effectiveness).IsGreaterThan(0.0);
        await Assert.That(effectiveness).IsLessThanOrEqualTo(1.0);
    }

    [Test]
    public async Task ShipCombatStats_IsDestroyed_WhenHullZero_ReturnsTrue()
    {
        // Arrange
        var stats = new ShipCombatStats
        {
            Hull = 200,
            CurrentHull = 0
        };

        // Act & Assert
        await Assert.That(stats.IsDestroyed).IsTrue();
    }

    [Test]
    public async Task SpecialAbility_CanActivate_WhenCooldownZero_ReturnsTrue()
    {
        // Arrange
        var ability = new SpecialAbility
        {
            Cooldown = 3,
            CurrentCooldown = 0,
            IsActive = false
        };

        // Act & Assert
        await Assert.That(ability.CanActivate()).IsTrue();
    }

    [Test]
    public async Task SpecialAbility_Activate_WhenCanActivate_SetsActiveAndCooldown()
    {
        // Arrange
        var ability = new SpecialAbility
        {
            Cooldown = 3,
            CurrentCooldown = 0,
            IsActive = false
        };

        // Act
        ability.Activate();

        // Assert
        await Assert.That(ability.IsActive).IsTrue();
        await Assert.That(ability.CurrentCooldown).IsEqualTo(3);
    }

    [Test]
    public async Task CombatEnvironment_GetAccuracyModifier_ReturnsValidValue()
    {
        // Arrange
        var environment = new CombatEnvironment
        {
            Weather = WeatherCondition.Storm,
            Visibility = 0.5
        };

        // Act
        var modifier = environment.GetAccuracyModifier();

        // Assert
        await Assert.That(modifier).IsGreaterThan(0.0);
        await Assert.That(modifier).IsLessThanOrEqualTo(1.0);
    }

    [Test]
    public async Task AttackResult_CreateHit_ReturnsValidResult()
    {
        // Act
        var result = AttackResult.CreateHit(10, 20, true);

        // Assert
        await Assert.That(result.Hit).IsTrue();
        await Assert.That(result.ShieldDamage).IsEqualTo(10);
        await Assert.That(result.HullDamage).IsEqualTo(20);
        await Assert.That(result.Critical).IsTrue();
        await Assert.That(result.Description).IsEqualTo("Critical hit!");
    }

    [Test]
    public async Task AttackResult_Miss_ReturnsValidResult()
    {
        // Act
        var result = AttackResult.Miss;

        // Assert
        await Assert.That(result.Hit).IsFalse();
        await Assert.That(result.Description).IsEqualTo("Attack missed");
    }

    [Test]
    public async Task CombatState_GetActiveShips_ReturnsOnlyNonDestroyedShips()
    {
        // Arrange
        var state = new CombatState
        {
            AttackerShips = new List<CombatShip>
            {
                CreateCombatShip("Ship1", true),
                CreateCombatShip("Ship2", true)
            },
            DefenderShips = new List<CombatShip>
            {
                CreateCombatShip("Ship3", false)
            }
        };

        // Destroy one ship
        state.AttackerShips[0].Stats.CurrentHull = 0;

        // Act
        var activeShips = state.GetActiveShips();

        // Assert
        await Assert.That(activeShips.Count).IsEqualTo(2); // 1 attacker + 1 defender
        await Assert.That(activeShips.Any(s => s.Stats.IsDestroyed)).IsFalse();
    }

    [Test]
    public async Task CombatShip_InitializeCombat_SetsProperValues()
    {
        // Arrange
        var ship = CreateCombatShip("TestShip", true);

        // Act
        ship.InitializeCombat();

        // Assert
        await Assert.That(ship.Stats.CurrentShields).IsEqualTo(ship.Stats.Shields);
        await Assert.That(ship.Stats.CurrentHull).IsEqualTo(ship.Stats.Hull);
        await Assert.That(ship.Initiative).IsGreaterThan(0);
    }

    private CombatShip CreateCombatShip(string name, bool isAttacker)
    {
        return new CombatShip
        {
            Id = Guid.NewGuid(),
            Name = name,
            OwnerId = Guid.NewGuid(),
            IsAttacker = isAttacker,
            Stats = new ShipCombatStats
            {
                Attack = 10,
                Defense = 5,
                Shields = 50,
                Hull = 100,
                Speed = 2,
                Range = 1,
                Accuracy = 0.8,
                CurrentShields = 50,
                CurrentHull = 100
            }
        };
    }
}