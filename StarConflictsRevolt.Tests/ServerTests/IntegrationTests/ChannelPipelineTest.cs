namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

// Temporarily commented out due to NUnit/TUnit compatibility issues
/*
// [TestFixture] // Commented out for TUnit compatibility
public class ChannelPipelineTest
{
    private TestApiHost _testHost = null!;

    // [SetUp] // Commented out for TUnit compatibility
    public async Task Setup()
    {
        _testHost = await TestApiHost.CreateAsync();
    }

    // [TearDown] // Commented out for TUnit compatibility
    public async Task TearDown()
    {
        await _testHost.DisposeAsync();
    }

    [Test]
    public async Task CommandQueue_ShouldUseChannelMechanism()
    {
        // Arrange
        var commandQueue = _testHost.Services.GetRequiredService<CommandQueue<IGameEvent>>();
        var worldId = Guid.NewGuid();
        var testEvent = new MoveFleetEvent
        {
            FleetId = Guid.NewGuid(),
            FromPlanetId = Guid.NewGuid(),
            ToPlanetId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid()
        };

        // Act: Enqueue command
        commandQueue.Enqueue(worldId, testEvent);

        // Assert: Command should be in queue
        var queue = commandQueue.GetOrCreateQueue(worldId);
        Assert.That(queue.Count, Is.EqualTo(1), "Command should be enqueued");

        // Act: Dequeue command
        var dequeued = commandQueue.TryDequeue(worldId, out var dequeuedEvent);

        // Assert: Command should be dequeued
        Assert.That(dequeued, Is.True, "Command should be dequeued successfully");
        Assert.That(dequeuedEvent, Is.EqualTo(testEvent), "Dequeued event should match original");
        Assert.That(queue.Count, Is.EqualTo(0), "Queue should be empty after dequeue");
    }

    [Test]
    public async Task EventStore_ShouldUseChannelMechanism()
    {
        // Arrange
        var eventStore = _testHost.Services.GetRequiredService<IEventStore>();
        var worldId = Guid.NewGuid();
        var testEvent = new MoveFleetEvent
        {
            FleetId = Guid.NewGuid(),
            FromPlanetId = Guid.NewGuid(),
            ToPlanetId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid()
        };

        var receivedEvents = new List<EventEnvelope>();
        var eventReceived = new TaskCompletionSource<EventEnvelope>();

        // Subscribe to events
        await eventStore.SubscribeAsync((envelope) =>
        {
            receivedEvents.Add(envelope);
            eventReceived.SetResult(envelope);
            return Task.CompletedTask;
        }, CancellationToken.None);

        // Act: Publish event
        await eventStore.PublishAsync(worldId, testEvent);

        // Assert: Event should be received through channel
        var receivedEnvelope = await eventReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(receivedEnvelope.WorldId, Is.EqualTo(worldId), "Event should have correct world ID");
        Assert.That(receivedEnvelope.Event, Is.EqualTo(testEvent), "Event should match original");
        Assert.That(receivedEvents.Count, Is.EqualTo(1), "Should receive exactly one event");
    }

    [Test]
    public async Task CompletePipeline_ShouldFollowChannelFlow()
    {
        // Arrange
        var commandQueue = _testHost.Services.GetRequiredService<CommandQueue<IGameEvent>>();
        var eventStore = _testHost.Services.GetRequiredService<IEventStore>();
        var gameUpdateService = _testHost.Services.GetRequiredService<GameUpdateService>();
        var sessionManager = _testHost.Services.GetRequiredService<SessionAggregateManager>();

        var worldId = Guid.NewGuid();
        var testEvent = new MoveFleetEvent
        {
            FleetId = Guid.NewGuid(),
            FromPlanetId = Guid.NewGuid(),
            ToPlanetId = Guid.NewGuid(),
            PlayerId = Guid.NewGuid()
        };

        var eventsReceived = new List<EventEnvelope>();
        var eventReceived = new TaskCompletionSource<EventEnvelope>();

        // Subscribe to events
        await eventStore.SubscribeAsync((envelope) =>
        {
            eventsReceived.Add(envelope);
            eventReceived.SetResult(envelope);
            return Task.CompletedTask;
        }, CancellationToken.None);

        // Create session for the world
        await sessionManager.CreateSessionAsync(worldId, "Test Session");

        // Act 1: Enqueue command (simulates HTTP endpoint)
        commandQueue.Enqueue(worldId, testEvent);

        // Assert 1: Command should be in queue
        var queue = commandQueue.GetOrCreateQueue(worldId);
        Assert.That(queue.Count, Is.EqualTo(1), "Command should be enqueued");

        // Act 2: Wait for GameUpdateService to process the command
        // This simulates the background service processing the command queue
        await Task.Delay(1000); // Give time for processing

        // Assert 2: Command should be processed and removed from queue
        Assert.That(queue.Count, Is.EqualTo(0), "Command should be processed and removed from queue");

        // Assert 3: Event should be published to event store
        var receivedEnvelope = await eventReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.That(receivedEnvelope.WorldId, Is.EqualTo(worldId), "Event should have correct world ID");
        Assert.That(receivedEnvelope.Event, Is.EqualTo(testEvent), "Event should match original command");

        // Assert 4: Event should be stored in database
        var storedEvents = eventStore.GetEventsForWorld(worldId);
        Assert.That(storedEvents.Count(), Is.GreaterThan(0), "Event should be stored in database");
        Assert.That(storedEvents.Any(e => e.Event is MoveFleetEvent), "MoveFleetEvent should be in database");
    }

    [Test]
    public async Task MultipleCommands_ShouldBeProcessedInOrder()
    {
        // Arrange
        var commandQueue = _testHost.Services.GetRequiredService<CommandQueue<IGameEvent>>();
        var eventStore = _testHost.Services.GetRequiredService<IEventStore>();
        var sessionManager = _testHost.Services.GetRequiredService<SessionAggregateManager>();

        var worldId = Guid.NewGuid();
        var events = new List<IGameEvent>
        {
            new MoveFleetEvent { FleetId = Guid.NewGuid(), FromPlanetId = Guid.NewGuid(), ToPlanetId = Guid.NewGuid(), PlayerId = Guid.NewGuid() },
            new BuildStructureEvent { PlanetId = Guid.NewGuid(), StructureType = "Factory", PlayerId = Guid.NewGuid() },
            new AttackEvent { AttackerFleetId = Guid.NewGuid(), DefenderFleetId = Guid.NewGuid(), LocationPlanetId = Guid.NewGuid(), PlayerId = Guid.NewGuid() }
        };

        var receivedEvents = new List<EventEnvelope>();
        var allEventsReceived = new TaskCompletionSource<bool>();

        // Subscribe to events
        await eventStore.SubscribeAsync((envelope) =>
        {
            receivedEvents.Add(envelope);
            if (receivedEvents.Count == events.Count)
            {
                allEventsReceived.SetResult(true);
            }
            return Task.CompletedTask;
        }, CancellationToken.None);

        // Create session
        await sessionManager.CreateSessionAsync(worldId, "Test Session");

        // Act: Enqueue multiple commands
        foreach (var @event in events)
        {
            commandQueue.Enqueue(worldId, @event);
        }

        // Assert: All commands should be processed
        await allEventsReceived.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.That(receivedEvents.Count, Is.EqualTo(events.Count), "All events should be processed");
        
        // Verify events are in correct order (FIFO)
        for (int i = 0; i < events.Count; i++)
        {
            Assert.That(receivedEvents[i].Event.GetType(), Is.EqualTo(events[i].GetType()), 
                $"Event {i} should be of correct type");
        }
    }

    [Test]
    public async Task ChannelBackpressure_ShouldHandleHighLoad()
    {
        // Arrange
        var commandQueue = _testHost.Services.GetRequiredService<CommandQueue<IGameEvent>>();
        var eventStore = _testHost.Services.GetRequiredService<IEventStore>();
        var sessionManager = _testHost.Services.GetRequiredService<SessionAggregateManager>();

        var worldId = Guid.NewGuid();
        var commandCount = 100;
        var receivedEvents = new List<EventEnvelope>();
        var allEventsReceived = new TaskCompletionSource<bool>();

        // Subscribe to events
        await eventStore.SubscribeAsync((envelope) =>
        {
            receivedEvents.Add(envelope);
            if (receivedEvents.Count == commandCount)
            {
                allEventsReceived.SetResult(true);
            }
            return Task.CompletedTask;
        }, CancellationToken.None);

        // Create session
        await sessionManager.CreateSessionAsync(worldId, "Test Session");

        // Act: Enqueue many commands rapidly
        var tasks = new List<Task>();
        for (int i = 0; i < commandCount; i++)
        {
            var @event = new MoveFleetEvent
            {
                FleetId = Guid.NewGuid(),
                FromPlanetId = Guid.NewGuid(),
                ToPlanetId = Guid.NewGuid(),
                PlayerId = Guid.NewGuid()
            };
            
            tasks.Add(Task.Run(() => commandQueue.Enqueue(worldId, @event)));
        }

        // Wait for all enqueue operations
        await Task.WhenAll(tasks);

        // Assert: All commands should be processed despite high load
        await allEventsReceived.Task.WaitAsync(TimeSpan.FromSeconds(30));
        Assert.That(receivedEvents.Count, Is.EqualTo(commandCount), "All events should be processed under load");
        
        // Verify queue is empty
        var queue = commandQueue.GetOrCreateQueue(worldId);
        Assert.That(queue.Count, Is.EqualTo(0), "Queue should be empty after processing all commands");
    }
}
*/ 