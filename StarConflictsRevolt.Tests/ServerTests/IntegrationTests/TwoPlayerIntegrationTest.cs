using System.Collections.Concurrent;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Eventing;
using StarConflictsRevolt.Server.WebApi.Security;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class TwoPlayerIntegrationTest
{
    [Test]
    [Timeout(30_000)]
    public async Task TwoHumanPlayers_SessionCreationAndJoining_NoAIActions(CancellationToken cancellationToken)
    {
        // Create test host with MockEventStore and minimal services
        var testHost = new TestHostApplication(includeClientServices: true);
        
        // Note: We can't easily replace the IEventStore after the application is built
        // The MockEventStore will be used by default in the test environment
        
        await testHost.StartServerAsync(cancellationToken);
        
        try
        {
            // Create HTTP client
            var httpClient = testHost.GetHttpClient();
            
            // Create SignalR connections
            var frankHubConnection = new HubConnectionBuilder()
                .WithUrl($"http://localhost:{testHost.Port}/gamehub")
                .Build();
            
            var mariellHubConnection = new HubConnectionBuilder()
                .WithUrl($"http://localhost:{testHost.Port}/gamehub")
                .Build();
            
            // Start connections
            await frankHubConnection.StartAsync(cancellationToken);
            await mariellHubConnection.StartAsync(cancellationToken);
            
            await Context.Current.OutputWriter.WriteLineAsync("[TEST] Both SignalR connections established");
            
            // Collect deltas
            var frankReceivedDeltas = new ConcurrentBag<GameObjectUpdate>();
            var mariellReceivedDeltas = new ConcurrentBag<GameObjectUpdate>();
            
            frankHubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", deltas =>
            {
                foreach (var delta in deltas)
                {
                    frankReceivedDeltas.Add(delta);
                }
                return Task.CompletedTask;
            });
            
            mariellHubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", deltas =>
            {
                foreach (var delta in deltas)
                {
                    mariellReceivedDeltas.Add(delta);
                }
                return Task.CompletedTask;
            });
            
            await Context.Current.OutputWriter.WriteLineAsync("[TEST] SignalR handlers registered");
            
            // Create session
            var createSessionRequest = new CreateSessionRequest
            {
                SessionName = "TestSession",
                SessionType = "Multiplayer"
            };
            
            var createResponse = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest, cancellationToken);
            createResponse.EnsureSuccessStatusCode();
            
            var sessionResponse = await createResponse.Content.ReadFromJsonAsync<SessionResponse>(cancellationToken: cancellationToken);
            await Assert.That(sessionResponse).IsNotNull();
            await Assert.That(sessionResponse!.SessionId).IsNotEqualTo(Guid.Empty);
            
            await Context.Current.OutputWriter.WriteLineAsync($"[TEST] Session created: {sessionResponse.SessionId}");
            
            // Join session
            var joinRequest = new CreateSessionRequest
            {
                SessionName = "TestSession",
                SessionType = "Multiplayer"
            };
            
            var joinResponse = await httpClient.PostAsJsonAsync($"/game/session/{sessionResponse.SessionId}/join", joinRequest, cancellationToken);
            joinResponse.EnsureSuccessStatusCode();
            
            var joinSessionResponse = await joinResponse.Content.ReadFromJsonAsync<SessionResponse>(cancellationToken: cancellationToken);
            await Assert.That(joinSessionResponse).IsNotNull();
            
            await Context.Current.OutputWriter.WriteLineAsync($"[TEST] Mariell joined session: {joinSessionResponse!.SessionId}");
            
            // Join SignalR groups
            await frankHubConnection.InvokeAsync("JoinWorld", sessionResponse.SessionId.ToString(), cancellationToken);
            await mariellHubConnection.InvokeAsync("JoinWorld", sessionResponse.SessionId.ToString(), cancellationToken);
            
            await Context.Current.OutputWriter.WriteLineAsync("[TEST] Both players joined SignalR groups");
            
            // Perform a simple action to trigger updates
            var world = sessionResponse.World;
            if (world?.Galaxy?.StarSystems?.FirstOrDefault()?.Planets?.FirstOrDefault() is PlanetDto planet)
            {
                var buildCommand = new BuildStructureEvent(
                    PlayerId: Guid.NewGuid(),
                    PlanetId: planet.Id,
                    StructureType: "Mine"
                );
                
                var buildResponse = await httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionResponse.SessionId}", buildCommand, cancellationToken);
                buildResponse.EnsureSuccessStatusCode();
                await Context.Current.OutputWriter.WriteLineAsync($"[TEST] Build command sent for planet {planet.Id}");
            }
            
            // Wait for some time to allow hosted services to process and send updates
            await Task.Delay(2000, cancellationToken);
            
            await Context.Current.OutputWriter.WriteLineAsync($"[TEST] Frank received {frankReceivedDeltas.Count} deltas via SignalR");
            foreach (var delta in frankReceivedDeltas)
            {
                await Context.Current.OutputWriter.WriteLineAsync($"[TEST] Frank delta: {delta.Id} - {delta.Type}");
            }
            
            await Context.Current.OutputWriter.WriteLineAsync($"[TEST] Mariell received {mariellReceivedDeltas.Count} deltas via SignalR");
            foreach (var delta in mariellReceivedDeltas)
            {
                await Context.Current.OutputWriter.WriteLineAsync($"[TEST] Mariell delta: {delta.Id} - {delta.Type}");
            }
            
            // Verify that at least one player received updates
            await Assert.That(frankReceivedDeltas.Count > 0 || mariellReceivedDeltas.Count > 0).IsTrue();
            
            // Clean up
            await frankHubConnection.StopAsync(cancellationToken);
            await mariellHubConnection.StopAsync(cancellationToken);
        }
        finally
        {
            // Dispose the test host (which will stop the server)
            testHost.Dispose();
        }
    }
}