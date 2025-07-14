using System.Collections.Concurrent;
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Fleets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Galaxies;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Stars;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Structures;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class ChangeTrackerTests
{
    private readonly ConcurrentBag<string> _logSink = new();
    private readonly IServiceProvider _provider;

    public ChangeTrackerTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(new TestLoggerProvider(_logSink)).SetMinimumLevel(LogLevel.Debug));
        _provider = services.BuildServiceProvider();
    }

    [Test]
    public async Task Detects_Structure_Added()
    {
        var planetId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var oldPlanet = new Planet("A", 1, 1, 1, 1, 1, new List<Fleet>(), new List<Structure>())
        {
            Id = planetId
        };
        var newStructure = new Structure(StructureVariant.Mine, oldPlanet, playerId);
        var newPlanet = oldPlanet with { Structures = new List<Structure> { newStructure } };
        var oldSystem = new StarSystem(Guid.NewGuid(), "Sys", new List<Planet> { oldPlanet }, new Vector2(0, 0));
        var newSystem = oldSystem with { Planets = new List<Planet> { newPlanet } };
        var oldGalaxy = new Galaxy(new List<StarSystem> { oldSystem });
        var newGalaxy = oldGalaxy with { StarSystems = new List<StarSystem> { newSystem } };
        var oldWorld = new World(Guid.NewGuid(), oldGalaxy);
        var newWorld = new World(oldWorld.Id, newGalaxy);

        var deltas = ChangeTracker.ComputeDeltas(oldWorld, newWorld);
        await Assert.That(deltas.Any(d => d.Type == UpdateType.Added)).IsTrue();
    }

    [Test]
    public async Task Detects_Structure_Removed()
    {
        var planetId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var structure = new Structure(StructureVariant.Mine, null!, playerId);
        var oldPlanet = new Planet("A", 1, 1, 1, 1, 1, new List<Fleet>(), new List<Structure> { structure })
        {
            Id = planetId,
            Structures = new List<Structure> { structure }
        };
        var newPlanet = oldPlanet with { Structures = new List<Structure>() };
        var oldSystem = new StarSystem(Guid.NewGuid(), "Sys", new List<Planet> { oldPlanet }, new Vector2(0, 0));
        var newSystem = oldSystem with { Planets = new List<Planet> { newPlanet } };
        var oldGalaxy = new Galaxy(new List<StarSystem> { oldSystem });
        var newGalaxy = oldGalaxy with { StarSystems = new List<StarSystem> { newSystem } };
        var oldWorld = new World(Guid.NewGuid(), oldGalaxy);
        var newWorld = new World(oldWorld.Id, newGalaxy);

        var deltas = ChangeTracker.ComputeDeltas(oldWorld, newWorld);
        await Assert.That(deltas.Any(d => d.Type == UpdateType.Removed)).IsTrue();
    }

    [Test]
    public async Task SessionAggregate_Apply_BuildStructureEvent_MutatesWorld()
    {
        var logger = _provider.GetRequiredService<ILogger<SessionAggregate>>();
        var ownerId = Guid.NewGuid();
        var planet = new Planet("A", 1, 1, 1, 1, 1, new List<Fleet>(), new List<Structure>(), ownerId)
        {
            Id = Guid.NewGuid(),
            Structures = new List<Structure>()
        };
        var system = new StarSystem(Guid.NewGuid(), "Sys", new List<Planet> { planet }, new Vector2(0, 0));
        var galaxy = new Galaxy(new List<StarSystem> { system });
        var world = new World(Guid.NewGuid(), galaxy);
        var aggregate = new SessionAggregate(Guid.NewGuid(), world, logger);
        var buildEvent = new BuildStructureEvent(ownerId, planet.Id, StructureVariant.Mine.ToString());
        aggregate.Apply(buildEvent);
        // Fetch the mutated planet from the world
        var mutatedPlanet = aggregate.World.Galaxy.StarSystems.SelectMany(s => s.Planets).First(p => p.Id == planet.Id);
        await Assert.That(mutatedPlanet.Structures.Any(s => s.Variant == StructureVariant.Mine)).IsTrue();
    }

    [Test]
    public async Task CommandQueue_Enqueue_Dequeue_Logs()
    {
        var logger = _provider.GetRequiredService<ILogger<CommandQueue>>();
        var queue = new CommandQueue(logger);
        var sessionId = Guid.NewGuid();
        var testEvent = new BuildStructureEvent(Guid.NewGuid(), Guid.NewGuid(), "Mine");
        queue.Enqueue(sessionId, testEvent);
        var dequeued = queue.TryDequeue(sessionId, out var result);
        await Assert.That(dequeued).IsTrue();
        await Assert.That(result.Command).IsEqualTo(testEvent);
        await Assert.That(_logSink.Any(msg => msg.Contains("Enqueued command"))).IsTrue();
        await Assert.That(_logSink.Any(msg => msg.Contains("Dequeued command"))).IsTrue();
    }
}

// Logger provider for capturing logs (same as in integration test)