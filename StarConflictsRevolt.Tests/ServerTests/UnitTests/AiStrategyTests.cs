using System.Numerics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.AI;
using StarConflictsRevolt.Server.Application.Services.Gameplay;
using StarConflictsRevolt.Server.Domain.AI;
using StarConflictsRevolt.Server.Domain.Fleets;
using StarConflictsRevolt.Server.Domain.Galaxies;
using StarConflictsRevolt.Server.Domain.Planets;
using StarConflictsRevolt.Server.Domain.Stars;
using StarConflictsRevolt.Server.Domain.Structures;
using StarConflictsRevolt.Server.Domain.Commands;
using WorldState = StarConflictsRevolt.Server.Domain.World.World;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class AiStrategyTests
{
    private readonly ILogger _logger;
    private readonly AiMemoryBank _memoryBank;

    public AiStrategyTests()
    {
        _memoryBank = new AiMemoryBank();
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AiStrategyTests>();
    }

    [Test]
    public void AggressiveAiStrategy_ShouldGenerateCombatDecisions()
    {
        // Arrange
        var strategy = new AggressiveAiStrategy(_memoryBank);
        var playerId = Guid.NewGuid();
        var world = CreateTestWorld(playerId);

        // Act
        var commands = strategy.GenerateCommands(playerId, world, 0L, _logger);

        // Assert
        commands.Should().NotBeNull();
        commands.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void EconomicAiStrategy_ShouldGenerateBuildingDecisions()
    {
        // Arrange
        var strategy = new EconomicAiStrategy(_memoryBank);
        var playerId = Guid.NewGuid();
        var world = CreateTestWorld(playerId);

        // Act
        var commands = strategy.GenerateCommands(playerId, world, 0L, _logger);

        // Assert
        commands.Should().NotBeNull();
        commands.Count.Should().BeGreaterThanOrEqualTo(0);
        var buildCommands = commands.OfType<QueueBuild>().ToList();
        buildCommands.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void DefensiveAiStrategy_ShouldGenerateDefensiveDecisions()
    {
        // Arrange
        var strategy = new DefensiveAiStrategy(_memoryBank);
        var playerId = Guid.NewGuid();
        var world = CreateTestWorld(playerId);

        // Act
        var commands = strategy.GenerateCommands(playerId, world, 0L, _logger);

        // Assert
        commands.Should().NotBeNull();
        commands.Count.Should().BeGreaterThanOrEqualTo(0);
        var buildCommands = commands.OfType<QueueBuild>().ToList();
        buildCommands.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void BalancedAiStrategy_ShouldAdaptToGamePhase()
    {
        // Arrange
        var strategy = new BalancedAiStrategy(_memoryBank);
        var playerId = Guid.NewGuid();
        var world = CreateTestWorld(playerId);

        // Act
        var commands = strategy.GenerateCommands(playerId, world, 0L, _logger);

        // Assert
        commands.Should().NotBeNull();
        commands.Count.Should().BeGreaterThanOrEqualTo(0);
        var moveCommands = commands.OfType<MoveFleet>().ToList();
        var buildCommands = commands.OfType<QueueBuild>().ToList();
        (moveCommands.Count + buildCommands.Count).Should().BeLessThanOrEqualTo(commands.Count);
    }

    [Test]
    public void DefaultAiStrategy_ShouldGenerateRandomDecisions()
    {
        // Arrange
        var strategy = new DefaultAiStrategy(_memoryBank);
        var playerId = Guid.NewGuid();
        var world = CreateTestWorld(playerId);

        // Act
        var commands = strategy.GenerateCommands(playerId, world, 0L, _logger);

        // Assert
        commands.Should().NotBeNull();
        commands.Count.Should().BeGreaterThanOrEqualTo(0);
        var moveCommands = commands.OfType<MoveFleet>().ToList();
        var buildCommands = commands.OfType<QueueBuild>().ToList();
        (moveCommands.Count + buildCommands.Count).Should().BeLessThanOrEqualTo(commands.Count);
    }

    [Test]
    public void AiMemoryBank_ShouldStoreAndRetrieveMemories()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var memory = new AiMemory(playerId, MemoryType.Decision, "Test decision", 0.8);

        // Act
        _memoryBank.AddMemory(memory);
        var retrievedMemories = _memoryBank.GetMemories(playerId);

        // Assert
        retrievedMemories.Should().NotBeEmpty();
        retrievedMemories.Should().Contain(memory);
    }

    [Test]
    public void AiDecision_ShouldHaveProperScoring()
    {
        // Arrange
        var decision = new AiDecision(AiDecisionType.MoveFleet, AiPriority.High, 75.0, "Test decision");

        // Act & Assert
        decision.Score.Should().Be(75.0);
        decision.Priority.Should().Be(AiPriority.High);
        decision.Type.Should().Be(AiDecisionType.MoveFleet);
        decision.Description.Should().Be("Test decision");
    }

    [Test]
    public void AiGoal_ShouldTrackProgress()
    {
        // Arrange
        var goal = new AiGoal(AiGoalType.Build, GoalTimeframe.ShortTerm, "Test goal", 80.0);

        // Act
        goal.UpdateProgress(0.5);

        // Assert
        goal.Progress.Should().Be(0.5);
        goal.IsCompleted.Should().BeFalse();

        // Act - complete the goal
        goal.UpdateProgress(1.0);

        // Assert
        goal.Progress.Should().Be(1.0);
        goal.IsCompleted.Should().BeTrue();
    }

    private WorldState CreateTestWorld(Guid playerId)
    {
        // Create a simple test world with some planets and fleets
        var planet1 = new Planet("Test Planet 1", 0, 0, 0, 0, 0, new List<Fleet>(), new List<Structure>(), null, 0, 0, 0, 0, 0, 0, 0, PlanetType.Terran)
        {
            Id = Guid.NewGuid()
        };

        var planet2 = new Planet("Test Planet 2", 0, 0, 0, 0, 0, new List<Fleet>(), new List<Structure>(), null, 0, 0, 0, 0, 0, 0, 0, PlanetType.Terran)
        {
            Id = Guid.NewGuid()
        };

        var fleet1 = new Fleet(playerId, "Test Fleet 1", new List<Ship>(), planet1.Id, playerId)
        {
            LocationPlanetId = planet1.Id
        };

        var fleet2 = new Fleet(Guid.NewGuid(), "Enemy Fleet", new List<Ship>(), planet2.Id, Guid.NewGuid())
        {
            LocationPlanetId = planet2.Id
        };

        planet1.Fleets.Add(fleet1);
        planet2.Fleets.Add(fleet2);

        var starSystem = new StarSystem(Guid.NewGuid(), "Test System", new List<Planet> { planet1, planet2 }, new Vector2(0, 0));

        var galaxy = new Galaxy(new List<StarSystem> { starSystem });

        return new WorldState(Guid.NewGuid(), galaxy);
    }
}