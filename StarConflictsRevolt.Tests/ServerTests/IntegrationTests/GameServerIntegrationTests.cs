using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class GameServerIntegrationTests
{
    [Test]
    [Timeout(30_000)]
    public async Task GameServer_CanStartAndServeRequests(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(cancellationToken);

        // Test that the server is running and can serve requests
        var httpClient = testHost.GetHttpClient();

        // Test basic connectivity
        var response = await httpClient.GetAsync("/health", cancellationToken);
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameServer_CanCreateSessionAndJoinViaSignalR(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(true);
        await testHost.StartServerAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // Create a session
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionResponse = await httpClient.PostAsJsonAsync("/game/session", new { SessionName = sessionName, SessionType = "Multiplayer" }, cancellationToken);

        createSessionResponse.EnsureSuccessStatusCode();
        var sessionObj = await createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>(cancellationToken);
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");

        // Output the response for debugging
        await Context.Current.OutputWriter.WriteLineAsync($"Create session request: {createSessionResponse.ReasonPhrase} ({createSessionResponse.StatusCode})");
        await Context.Current.OutputWriter.WriteLineAsync($"Session created with ID: {sessionId}");

        // Connect to SignalR and join the session
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(testHost.GetGameServerHubUrl())
            .WithAutomaticReconnect()
            .Build();

        await hubConnection.StartAsync(cancellationToken);
        await hubConnection.SendAsync("JoinWorld", sessionId.ToString(), cancellationToken);

        // Verify we can receive updates
        var receivedUpdates = new List<GameObjectUpdate>();
        hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", updates => receivedUpdates.AddRange(updates));

        // Perform a simple action to trigger updates
        if (sessionObj?.World?.Galaxy?.StarSystems?.FirstOrDefault()?.Planets?.FirstOrDefault() is PlanetDto planet)
        {
            var buildCommand = new BuildStructureEvent(
                PlayerId: Guid.NewGuid(),
                PlanetId: planet.Id,
                StructureType: "Mine"
            );
            
            var buildResponse = await httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", buildCommand, cancellationToken);
            buildResponse.EnsureSuccessStatusCode();
            await Context.Current.OutputWriter.WriteLineAsync($"[TEST] Build command sent for planet {planet.Id}");
        }

        // Wait a moment for initial world state and updates
        await Task.Delay(2000, cancellationToken);

        await hubConnection.StopAsync(cancellationToken);
        await Assert.That(receivedUpdates).IsNotEmpty();
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameServer_CanUseRavenDbForPersistence(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(cancellationToken);

        using var session = testHost.DocumentStore.OpenAsyncSession();

        var testEntity = new TestEntity { Name = "Test", Value = 42 };
        await session.StoreAsync(testEntity, cancellationToken);
        await session.SaveChangesAsync(cancellationToken);

        var loaded = await session.LoadAsync<TestEntity>(testEntity.Id, cancellationToken);
        await Assert.That(loaded).IsNotNull();
        await Assert.That(loaded!.Name).IsEqualTo("Test");
        await Assert.That(loaded.Value).IsEqualTo(42);
    }



    private class TestEntity
    {
        public string Id { get; } = default!;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}