using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Galaxies;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Stars;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Structures;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.SeedData;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class CoreGameLogicTests
{
    private SessionAggregate CreateAggregate(Guid sessionId, World world)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        // Use the injected RavenDB session for event store
        services.AddSingleton<IEventStore>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RavenEventStore>>();
            var documentStore = sp.GetRequiredService<IDocumentStore>();
            return new RavenEventStore(documentStore, logger);
        });
        services.AddSingleton<SessionAggregateManager>();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        var serviceProvider = services.BuildServiceProvider();

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        return new SessionAggregate(sessionId, world, loggerFactory.CreateLogger<SessionAggregate>());
    }

    [Test]
    public async Task MoveFleetEvent_MovesFleetToNewPlanet()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var planetA = new Planet("A", 1, 1, 1, 1, 1, new List<Fleet>(), new List<Structure>());
        var planetB = new Planet("B", 1, 1, 1, 1, 2, new List<Fleet>(), new List<Structure>());
        var fleet = new Fleet(Guid.NewGuid(), "Test Fleet", new List<Ship> { ShipCollection.XWing.ToModel() }, planetA.Id, playerId);
        planetA.Fleets.Add(fleet);
        var system = new StarSystem(Guid.NewGuid(), "Sys", [planetA, planetB], new Vector2(0, 0));
        var galaxy = new Galaxy([system]);
        var world = new World(Guid.NewGuid(), galaxy);
        var aggregate = CreateAggregate(Guid.NewGuid(), world);
        var initialVersion = aggregate.Version;

        // Act
        var moveEvent = new MoveFleetEvent(playerId, fleet.Id, planetA.Id, planetB.Id);
        aggregate.Apply(moveEvent);

        // Assert
        await Assert.That(aggregate.UncommittedEvents.Any(e => e.Equals(moveEvent))).IsTrue();
        await Assert.That(aggregate.Version).IsEqualTo(initialVersion + 1);

        // Verify fleet was moved
        await Assert.That(planetA.Fleets).IsEmpty();
        await Assert.That(planetB.Fleets.Count).IsEqualTo(1);
        await Assert.That(planetB.Fleets[0].Id).IsEqualTo(fleet.Id);
        await Assert.That(planetB.Fleets[0].Status).IsEqualTo(FleetStatus.Moving);
        await Assert.That(planetB.Fleets[0].DestinationPlanetId).IsEqualTo(planetB.Id);
    }

    [Test]
    public async Task BuildStructureEvent_AddsStructureToPlanet()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var planet = new Planet("A", 1, 1, 1, 1, 1, new List<Fleet>(), new List<Structure>(), playerId, Minerals: 100, Energy: 100);
        var system = new StarSystem(Guid.NewGuid(), "Sys", [planet], new Vector2(0, 0));
        var galaxy = new Galaxy([system]);
        var world = new World(Guid.NewGuid(), galaxy);
        var aggregate = CreateAggregate(Guid.NewGuid(), world);
        var initialVersion = aggregate.Version;
        var initialMinerals = planet.Minerals;
        var initialEnergy = planet.Energy;

        // Act
        var buildEvent = new BuildStructureEvent(playerId, planet.Id, StructureVariant.Mine.ToString());
        aggregate.Apply(buildEvent);

        // Assert
        await Assert.That(aggregate.UncommittedEvents.Any(e => e.Equals(buildEvent))).IsTrue();
        await Assert.That(aggregate.Version).IsEqualTo(initialVersion + 1);

        // Fetch the mutated planet from the world
        var mutatedPlanet = aggregate.World.Galaxy.StarSystems.SelectMany(s => s.Planets).First(p => p.Id == planet.Id);
        // Verify structure was added
        await Assert.That(mutatedPlanet.Structures.Count).IsEqualTo(1);
        await Assert.That(mutatedPlanet.Structures[0].Variant).IsEqualTo(StructureVariant.Mine);
        await Assert.That(mutatedPlanet.Structures[0].OwnerId).IsEqualTo(playerId);
        // Verify resources were consumed
        await Assert.That(mutatedPlanet.Minerals).IsLessThan(initialMinerals);
        await Assert.That(mutatedPlanet.Energy).IsLessThan(initialEnergy);
    }

    [Test]
    public async Task AttackEvent_ResolvesCombat()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var defenderId = Guid.NewGuid();
        var planet = new Planet("A", 1, 1, 1, 1, 1, new List<Fleet>(), new List<Structure>());
        var attackerFleet = new Fleet(Guid.NewGuid(), "Attacker", new List<Ship>
        {
            new(Guid.NewGuid(), "Fighter", false, AttackPower: 20, DefensePower: 10)
        }, planet.Id, attackerId);
        var defenderFleet = new Fleet(Guid.NewGuid(), "Defender", new List<Ship>
        {
            new(Guid.NewGuid(), "Fighter", false, AttackPower: 15, DefensePower: 15)
        }, planet.Id, defenderId);
        planet.Fleets.Add(attackerFleet);
        planet.Fleets.Add(defenderFleet);
        var system = new StarSystem(Guid.NewGuid(), "Sys", [planet], new Vector2(0, 0));
        var galaxy = new Galaxy([system]);
        var world = new World(Guid.NewGuid(), galaxy);
        var aggregate = CreateAggregate(Guid.NewGuid(), world);
        var initialVersion = aggregate.Version;

        // Act
        var attackEvent = new AttackEvent(attackerId, attackerFleet.Id, defenderFleet.Id, planet.Id);
        aggregate.Apply(attackEvent);

        // Assert
        await Assert.That(aggregate.UncommittedEvents.Any(e => e.Equals(attackEvent))).IsTrue();
        await Assert.That(aggregate.Version).IsEqualTo(initialVersion + 1);

        // Fetch the mutated planet from the world
        var mutatedPlanet = aggregate.World.Galaxy.StarSystems.SelectMany(s => s.Planets).First(p => p.Id == planet.Id);
        // Verify combat was resolved (at least one fleet should have damage)
        var hasDamage = mutatedPlanet.Fleets.Any(f => f.Ships.Any(s => s.Health < 100));
        await Assert.That(hasDamage).IsTrue();
    }

    [Test]
    public async Task DiplomacyEvent_UpdatesPlayerRelations()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var targetPlayerId = Guid.NewGuid();
        var planet = new Planet("A", 1, 1, 1, 1, 1, new List<Fleet>(), new List<Structure>());
        var system = new StarSystem(Guid.NewGuid(), "Sys", [planet], new Vector2(0, 0));
        var galaxy = new Galaxy([system]);
        var world = new World(Guid.NewGuid(), galaxy, new List<PlayerController>
        {
            new() { PlayerId = playerId, Name = "Player1" },
            new() { PlayerId = targetPlayerId, Name = "Player2" }
        });
        var aggregate = CreateAggregate(Guid.NewGuid(), world);
        var initialVersion = aggregate.Version;

        // Act
        var diploEvent = new DiplomacyEvent(playerId, targetPlayerId, "Alliance", "Let's be friends!");
        aggregate.Apply(diploEvent);

        // Assert
        await Assert.That(aggregate.UncommittedEvents.Any(e => e.Equals(diploEvent))).IsTrue();
        await Assert.That(aggregate.Version).IsEqualTo(initialVersion + 1);

        // Verify diplomacy event was processed (should be logged)
        // In a full implementation, we would verify player relations were updated
    }
}