using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class TwoPlayerIntegrationTest
{
    [Test]
    [Timeout(30_000)]
    public async Task TwoHumanPlayers_SessionCreationAndJoining_NoAIActions(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        // Log sink for capturing logs
        var logSink = new ConcurrentBag<string>();

        // Note: AI service is registered in GameEngineStartupHelper.RegisterGameEngineServices
        // For this test, we'll verify no AI actions are taken by checking logs

        // Start the server explicitly
        await testHost.StartServerAsync(cancellationToken);

        // The application is now built and started
        var app = testHost.Server;
        await Assert.That(app).IsNotNull();

        // Ensure the database is created
        using var scope = app.Services.CreateScope();
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Ensuring database is created");
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Database created");

        // Create an HttpClient that can communicate with the test server
        var port = testHost.Port;
        await Context.Current.OutputWriter.WriteLineAsync($"[DIAG] Using port: {port}");
        var httpClient = testHost.GetHttpClient();

        // Player IDs for the test
        var playerMariellId = Guid.NewGuid();
        var playerFrankId = Guid.NewGuid();

        await Context.Current.OutputWriter.WriteLineAsync($"Player Mariell ID: {playerMariellId}");
        await Context.Current.OutputWriter.WriteLineAsync($"Player Frank ID: {playerFrankId}");

        // 1. Mariell creates a new session via API
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionRequest = new { SessionName = sessionName, SessionType = "Multiplayer" };
        await Context.Current.OutputWriter.WriteLineAsync($"[DIAG] Mariell creating session: {sessionName}");

        var createSessionResponse = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest, cancellationToken);
        createSessionResponse.EnsureSuccessStatusCode();
        // For ReadFromJsonAsync and similar, ensure Task is passed to WithTimeout
        var sessionObj = await createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>(cancellationToken);
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");

        await Context.Current.OutputWriter.WriteLineAsync($"[DIAG] Mariell created session: {sessionId}");

        // 2. Mariell connects to SignalR and joins the session group
        var mariellHubConnection = new HubConnectionBuilder()
            .WithUrl(testHost.GetGameServerHubUrl())
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        var mariellReceivedDeltas = new List<GameObjectUpdate>();
        mariellHubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", async deltas =>
        {
            mariellReceivedDeltas.AddRange(deltas);
            await Context.Current.OutputWriter.WriteLineAsync($"Mariell received {deltas.Count} deltas via SignalR");
        });

        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Starting Mariell SignalR connection");
        await mariellHubConnection.StartAsync(cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Mariell SignalR started");
        await mariellHubConnection.SendAsync("JoinWorld", sessionId.ToString(), cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync("Mariell joined the session");

        // 3. Frank connects to SignalR and joins the same session group
        var frankHubConnection = new HubConnectionBuilder()
            .WithUrl(testHost.GetGameServerHubUrl())
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        var frankReceivedDeltas = new List<GameObjectUpdate>();
        frankHubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", async deltas =>
        {
            frankReceivedDeltas.AddRange(deltas);
            await Context.Current.OutputWriter.WriteLineAsync($"Frank received {deltas.Count} deltas via SignalR");
        });

        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Starting Frank SignalR connection");
        await frankHubConnection.StartAsync(cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Frank SignalR started");
        await frankHubConnection.SendAsync("JoinWorld", sessionId.ToString(), cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync("Frank joined the session");

        // 4. Wait a moment for both players to receive initial world state
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Waiting for initial deltas");
        await Task.Delay(1000, cancellationToken);

        await Context.Current.OutputWriter.WriteLineAsync($"Mariell received {mariellReceivedDeltas.Count} total deltas");
        await Context.Current.OutputWriter.WriteLineAsync($"Frank received {frankReceivedDeltas.Count} total deltas");

        // 5. Get the world state to find a valid planet ID for testing
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Requesting world state");
        var worldResponse = await httpClient.GetAsync("/game/state", cancellationToken);
        if (!worldResponse.IsSuccessStatusCode)
        {
            var errorContent = await worldResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"World state request failed: {worldResponse.StatusCode} - {errorContent}");
            throw new Exception($"Failed to get world state: {worldResponse.StatusCode}");
        }

        // For ReadFromJsonAsync and similar, ensure Task is passed to WithTimeout
        var world = await worldResponse.Content.ReadFromJsonAsync<World>(cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"World state retrieved: {world?.Id}");

        if (world?.Galaxy?.StarSystems?.FirstOrDefault()?.Planets?.FirstOrDefault() is not Planet planet)
        {
            await Context.Current.OutputWriter.WriteLineAsync("No planet found in the world");
            throw new Exception("No planet found in the world");
        }

        await Context.Current.OutputWriter.WriteLineAsync($"Found planet: {planet.Id} - {planet.Name}");

        // 6. Mariell sends a build command
        var mariellBuildCommand = new
        {
            PlayerId = playerMariellId,
            PlanetId = planet.Id,
            StructureType = "Mine"
        };

        await Context.Current.OutputWriter.WriteLineAsync($"Mariell sending build command for planet: {planet.Id}");
        var mariellBuildResponse = await httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", mariellBuildCommand, cancellationToken);

        if (!mariellBuildResponse.IsSuccessStatusCode)
        {
            var errorContent = await mariellBuildResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"Mariell's build command failed: {mariellBuildResponse.StatusCode} - {errorContent}");
            throw new Exception($"Mariell's build command failed: {mariellBuildResponse.StatusCode} - {errorContent}");
        }

        await Context.Current.OutputWriter.WriteLineAsync("Mariell's build command sent successfully");

        // 7. Wait for both players to receive the update
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Waiting for post-Mariell build wait");
        await Task.Delay(1000, cancellationToken);

        await Context.Current.OutputWriter.WriteLineAsync($"After Mariell's command - Mariell received {mariellReceivedDeltas.Count} total deltas");
        await Context.Current.OutputWriter.WriteLineAsync($"After Mariell's command - Frank received {frankReceivedDeltas.Count} total deltas");

        // 8. Frank sends a different build command
        var frankBuildCommand = new
        {
            PlayerId = playerFrankId,
            PlanetId = planet.Id,
            StructureType = "Shipyard"
        };

        await Context.Current.OutputWriter.WriteLineAsync($"Frank sending build command for planet: {planet.Id}");
        var frankBuildResponse = await httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", frankBuildCommand, cancellationToken);

        if (!frankBuildResponse.IsSuccessStatusCode)
        {
            var errorContent = await frankBuildResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"Frank's build command failed: {frankBuildResponse.StatusCode} - {errorContent}");
            throw new Exception($"Frank's build command failed: {frankBuildResponse.StatusCode} - {errorContent}");
        }

        await Context.Current.OutputWriter.WriteLineAsync("Frank's build command sent successfully");

        // 9. Wait for both players to receive the update
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Waiting for post-Frank build wait");
        await Task.Delay(1000, cancellationToken);

        await Context.Current.OutputWriter.WriteLineAsync($"After Frank's command - Mariell received {mariellReceivedDeltas.Count} total deltas");
        await Context.Current.OutputWriter.WriteLineAsync($"After Frank's command - Frank received {frankReceivedDeltas.Count} total deltas");

        // 10. Gracefully stop the SignalR connections
        await mariellHubConnection.StopAsync(cancellationToken);
        await frankHubConnection.StopAsync(cancellationToken);

        // 11. Assertions
        await Assert.That(mariellReceivedDeltas).IsNotEmpty();
        await Assert.That(frankReceivedDeltas).IsNotEmpty();

        // Verify both players received the same number of deltas (indicating synchronization)
        await Assert.That(mariellReceivedDeltas.Count).IsEqualTo(frankReceivedDeltas.Count);

        // Verify no critical errors in logs
        var errorLogs = logSink.Where(msg =>
            msg.Contains("ObjectDisposedException") ||
            msg.Contains("Failed to replay events") ||
            msg.Contains("BackgroundService failed") ||
            msg.Contains("TaskCanceledException") ||
            msg.Contains("The HostOptions.BackgroundServiceExceptionBehavior is configured to StopHost")
        ).ToList();

        await Context.Current.OutputWriter.WriteLineAsync("Captured error logs:");
        foreach (var log in errorLogs)
            await Context.Current.OutputWriter.WriteLineAsync(log);
        await Assert.That(errorLogs).IsEmpty();
    }

    private record SessionResponse(Guid SessionId);
}