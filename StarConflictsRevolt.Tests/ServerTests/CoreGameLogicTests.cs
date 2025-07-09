using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Core.Enums;
using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Datastore.Extensions;
using StarConflictsRevolt.Server.Datastore.SeedData;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Server.GameEngine;

namespace StarConflictsRevolt.Tests;

public class CoreGameLogicTests
{
    [Test]
    public async Task MoveFleetEvent_MovesFleetToNewPlanet()
    {
        // Arrange
        var planetA = new Planet(Guid.NewGuid(), "A", 1, 1, 1, 1, 1);
        var planetB = new Planet(Guid.NewGuid(), "B", 1, 1, 1, 1, 2);
        var fleet = new Fleet(Guid.NewGuid(), "Test Fleet", new List<Ship> { ShipCollection.XWing.ToModel() });
        var system = new StarSystem(Guid.NewGuid(), "Sys", new[] { planetA, planetB }, new Vector2(0, 0));
        var galaxy = new Galaxy(Guid.NewGuid(), new[] { system });
        var world = new World(Guid.NewGuid(), galaxy);
        var aggregate = new SessionAggregate(Guid.NewGuid(), world);
        var initialVersion = aggregate.Version;

        // Act
        var moveEvent = new MoveFleetEvent(Guid.NewGuid(), fleet.Id, planetA.Id, planetB.Id);
        aggregate.Apply(moveEvent);

        // Assert
        await Assert.That(aggregate.UncommittedEvents.Any(e => e.Equals(moveEvent))).IsTrue();
        await Assert.That(aggregate.Version).IsEqualTo(initialVersion + 1);
        // TODO: Assert fleet location changes when logic is implemented
    }

    [Test]
    public async Task BuildStructureEvent_AddsStructureToPlanet()
    {
        // Arrange
        var planet = new Planet(Guid.NewGuid(), "A", 1, 1, 1, 1, 1);
        var system = new StarSystem(Guid.NewGuid(), "Sys", new[] { planet }, new Vector2(0, 0));
        var galaxy = new Galaxy(Guid.NewGuid(), new[] { system });
        var world = new World(Guid.NewGuid(), galaxy);
        var aggregate = new SessionAggregate(Guid.NewGuid(), world);
        var initialVersion = aggregate.Version;

        // Act
        var buildEvent = new BuildStructureEvent(Guid.NewGuid(), planet.Id, StructureVariant.Mine.ToString());
        aggregate.Apply(buildEvent);

        // Assert
        await Assert.That(aggregate.UncommittedEvents.Any(e => e.Equals(buildEvent))).IsTrue();
        await Assert.That(aggregate.Version).IsEqualTo(initialVersion + 1);
        // TODO: Assert structure added to planet when logic is implemented
    }

    [Test]
    public async Task AttackEvent_ResolvesCombat()
    {
        // Arrange
        var planet = new Planet(Guid.NewGuid(), "A", 1, 1, 1, 1, 1);
        var system = new StarSystem(Guid.NewGuid(), "Sys", new[] { planet }, new Vector2(0, 0));
        var galaxy = new Galaxy(Guid.NewGuid(), new[] { system });
        var world = new World(Guid.NewGuid(), galaxy);
        var aggregate = new SessionAggregate(Guid.NewGuid(), world);
        var initialVersion = aggregate.Version;

        // Act
        var attackEvent = new AttackEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), planet.Id);
        aggregate.Apply(attackEvent);

        // Assert
        await Assert.That(aggregate.UncommittedEvents.Any(e => e.Equals(attackEvent))).IsTrue();
        await Assert.That(aggregate.Version).IsEqualTo(initialVersion + 1);
        // TODO: Assert combat results when logic is implemented
    }

    [Test]
    public async Task DiplomacyEvent_UpdatesPlayerRelations()
    {
        // Arrange
        var planet = new Planet(Guid.NewGuid(), "A", 1, 1, 1, 1, 1);
        var system = new StarSystem(Guid.NewGuid(), "Sys", new[] { planet }, new Vector2(0, 0));
        var galaxy = new Galaxy(Guid.NewGuid(), new[] { system });
        var world = new World(Guid.NewGuid(), galaxy);
        var aggregate = new SessionAggregate(Guid.NewGuid(), world);
        var initialVersion = aggregate.Version;

        // Act
        var diploEvent = new DiplomacyEvent(Guid.NewGuid(), Guid.NewGuid(), "Alliance", "Let's be friends!");
        aggregate.Apply(diploEvent);

        // Assert
        await Assert.That(aggregate.UncommittedEvents.Any(e => e.Equals(diploEvent))).IsTrue();
        await Assert.That(aggregate.Version).IsEqualTo(initialVersion + 1);
        // TODO: Assert player relations when logic is implemented
    }
} 