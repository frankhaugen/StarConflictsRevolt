using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using System.Collections.Concurrent;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Models;
using TUnit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class TwoPlayerIntegrationTest
{
    [Test]
    public async Task TwoHumanPlayers_SessionCreationAndJoining_NoAIActions()
    {
        // Log sink for capturing logs
        var logSink = new ConcurrentBag<string>();

        using var appBuilderHost = new FullIntegrationTestWebApplicationBuilder();
        
        // Add our log provider to capture all logs from the application
        appBuilderHost.LoggingBuilder.AddProvider(new TestLoggerProvider(logSink));
        appBuilderHost.LoggingBuilder.SetMinimumLevel(LogLevel.Debug);
        
        // Note: AI service is registered in GameEngineStartupHelper.RegisterGameEngineServices
        // For this test, we'll verify no AI actions are taken by checking logs

        WebApplication app;
        
        // Build the application with a timeout for startup
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Building application");
        app = await WithTimeout(Task.FromResult(appBuilderHost.Build()), "BuildAsync");
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Application built");
        
        await Assert.That(app).IsNotNull();
        
        // Ensure the database is created
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Ensuring database is created");
        await WithTimeout(Task.Run(() => dbContext.Database.EnsureCreatedAsync()), "EnsureCreatedAsync");
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Database created");
        
        await app.StartAsync();
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Starting app");
        
        // Create an HttpClient that can communicate with the test server
        var port = appBuilderHost.GetPort();
        await Context.Current.OutputWriter.WriteLineAsync($"[DIAG] Using port: {port}");
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{port}") };

        // === AUTHENTICATION: Obtain JWT token ===
        var testClientId = $"test-client-{Guid.NewGuid()}";
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Requesting token");
        var tokenResponse = await WithTimeout(httpClient.PostAsJsonAsync("/token", new { ClientId = testClientId, Secret = "test-secret" }), "Token request");
        tokenResponse.EnsureSuccessStatusCode();
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Token response received");
        // For ReadFromJsonAsync and similar, ensure Task is passed to WithTimeout
        var tokenObj = await WithTimeout(Task.Run(() => tokenResponse.Content.ReadFromJsonAsync<TokenResponse>()), "TokenResponse deserialization");
        if (tokenObj == null || string.IsNullOrEmpty(tokenObj.access_token))
            throw new Exception("Failed to obtain JWT token for test user");
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenObj.access_token);
        // === END AUTHENTICATION ===
        
        // Player IDs for the test
        var playerMariellId = Guid.NewGuid();
        var playerFrankId = Guid.NewGuid();
        
        await Context.Current.OutputWriter.WriteLineAsync($"Player Mariell ID: {playerMariellId}");
        await Context.Current.OutputWriter.WriteLineAsync($"Player Frank ID: {playerFrankId}");
        
        // 1. Mariell creates a new session via API
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionRequest = new { SessionName = sessionName, SessionType = "Multiplayer" };
        await Context.Current.OutputWriter.WriteLineAsync($"[DIAG] Mariell creating session: {sessionName}");
        
        var createSessionResponse = await WithTimeout(httpClient.PostAsJsonAsync("/game/session", createSessionRequest), "Create session");
        createSessionResponse.EnsureSuccessStatusCode();
        // For ReadFromJsonAsync and similar, ensure Task is passed to WithTimeout
        var sessionObj = await WithTimeout(Task.Run(() => createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>()), "SessionResponse deserialization");
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");
        
        await Context.Current.OutputWriter.WriteLineAsync($"[DIAG] Mariell created session: {sessionId}");

        // 2. Mariell connects to SignalR and joins the session group
        var mariellHubConnection = new HubConnectionBuilder()
            .WithUrl(appBuilderHost.GetGameServerHubUrl())
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
        await WithTimeout(mariellHubConnection.StartAsync(), "MariellHubConnection.StartAsync");
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Mariell SignalR started");
        await WithTimeout(mariellHubConnection.SendAsync("JoinWorld", sessionId.ToString()), "Mariell JoinWorld");
        await Context.Current.OutputWriter.WriteLineAsync("Mariell joined the session");

        // 3. Frank connects to SignalR and joins the same session group
        var frankHubConnection = new HubConnectionBuilder()
            .WithUrl(appBuilderHost.GetGameServerHubUrl())
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
        await WithTimeout(frankHubConnection.StartAsync(), "FrankHubConnection.StartAsync");
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Frank SignalR started");
        await WithTimeout(frankHubConnection.SendAsync("JoinWorld", sessionId.ToString()), "Frank JoinWorld");
        await Context.Current.OutputWriter.WriteLineAsync("Frank joined the session");

        // 4. Wait a moment for both players to receive initial world state
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Waiting for initial deltas");
        await WithTimeout(Task.Delay(1000), "Initial delta wait");
        
        await Context.Current.OutputWriter.WriteLineAsync($"Mariell received {mariellReceivedDeltas.Count} total deltas");
        await Context.Current.OutputWriter.WriteLineAsync($"Frank received {frankReceivedDeltas.Count} total deltas");

        // 5. Get the world state to find a valid planet ID for testing
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Requesting world state");
        var worldResponse = await WithTimeout(httpClient.GetAsync("/game/state"), "World state request");
        if (!worldResponse.IsSuccessStatusCode)
        {
            var errorContent = await worldResponse.Content.ReadAsStringAsync();
            await Context.Current.OutputWriter.WriteLineAsync($"World state request failed: {worldResponse.StatusCode} - {errorContent}");
            throw new Exception($"Failed to get world state: {worldResponse.StatusCode}");
        }
        
        // For ReadFromJsonAsync and similar, ensure Task is passed to WithTimeout
        var world = await WithTimeout(Task.Run(() => worldResponse.Content.ReadFromJsonAsync<World>()), "World deserialization");
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
        var mariellBuildResponse = await WithTimeout(httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", mariellBuildCommand), "Mariell build command");
        
        if (!mariellBuildResponse.IsSuccessStatusCode)
        {
            var errorContent = await mariellBuildResponse.Content.ReadAsStringAsync();
            await Context.Current.OutputWriter.WriteLineAsync($"Mariell's build command failed: {mariellBuildResponse.StatusCode} - {errorContent}");
            throw new Exception($"Mariell's build command failed: {mariellBuildResponse.StatusCode} - {errorContent}");
        }
        
        await Context.Current.OutputWriter.WriteLineAsync("Mariell's build command sent successfully");

        // 7. Wait for both players to receive the update
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Waiting for post-Mariell build wait");
        await WithTimeout(Task.Delay(1000), "Post-Mariell build wait");
        
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
        var frankBuildResponse = await WithTimeout(httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", frankBuildCommand), "Frank build command");
        
        if (!frankBuildResponse.IsSuccessStatusCode)
        {
            var errorContent = await frankBuildResponse.Content.ReadAsStringAsync();
            await Context.Current.OutputWriter.WriteLineAsync($"Frank's build command failed: {frankBuildResponse.StatusCode} - {errorContent}");
            throw new Exception($"Frank's build command failed: {frankBuildResponse.StatusCode} - {errorContent}");
        }
        
        await Context.Current.OutputWriter.WriteLineAsync("Frank's build command sent successfully");

        // 9. Wait for both players to receive the update
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Waiting for post-Frank build wait");
        await WithTimeout(Task.Delay(1000), "Post-Frank build wait");
        
        await Context.Current.OutputWriter.WriteLineAsync($"After Frank's command - Mariell received {mariellReceivedDeltas.Count} total deltas");
        await Context.Current.OutputWriter.WriteLineAsync($"After Frank's command - Frank received {frankReceivedDeltas.Count} total deltas");

        // 10. Gracefully stop the application and SignalR connections
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] Stopping SignalR connections and app");
        await WithTimeout(Task.Run(() => mariellHubConnection.StopAsync()), "MariellHubConnection.StopAsync");
        await WithTimeout(Task.Run(() => frankHubConnection.StopAsync()), "FrankHubConnection.StopAsync");
        await WithTimeout(Task.Run(() => app.StopAsync()), "app.StopAsync");
        await WithTimeout(Task.Run(() => app.DisposeAsync()), "app.DisposeAsync");
        await Context.Current.OutputWriter.WriteLineAsync("[DIAG] App and connections stopped");
        
        // 11. Assertions
        await Assert.That(mariellReceivedDeltas).IsNotEmpty();
        await Assert.That(frankReceivedDeltas).IsNotEmpty();
        
        // Both players should have received the same number of deltas
        await Assert.That(mariellReceivedDeltas.Count).IsEqualTo(frankReceivedDeltas.Count);
        
        // Check that we received the expected deltas (structures added to planet)
        var mariellStructureDelta = mariellReceivedDeltas.FirstOrDefault(d =>
            (d.Type == UpdateType.Added || d.Type == UpdateType.Changed) &&
            (
                (d.Data.HasValue && d.Data.Value.TryGetProperty("variant", out var variant) && variant.GetString() == "mine") ||
                (d.Data.HasValue && d.Data.Value.TryGetProperty("structureType", out var structureType) && structureType.GetString() == "Mine")
            )
        );
        await Assert.That(mariellStructureDelta).IsNotNull();
        
        var frankStructureDelta = frankReceivedDeltas.FirstOrDefault(d =>
            (d.Type == UpdateType.Added || d.Type == UpdateType.Changed) &&
            (
                (d.Data.HasValue && d.Data.Value.TryGetProperty("variant", out var variant) && variant.GetString() == "shipyard") ||
                (d.Data.HasValue && d.Data.Value.TryGetProperty("structureType", out var structureType) && structureType.GetString() == "Shipyard")
            )
        );
        await Assert.That(frankStructureDelta).IsNotNull();

        // 12. Assert no critical errors/warnings in logs
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
        
        // 13. Verify no AI actions were taken (no AI-related logs)
        var aiLogs = logSink.Where(msg =>
            msg.Contains("AI Player") ||
            msg.Contains("AiController") ||
            msg.Contains("AiTurnService")
        ).ToList();
        await Context.Current.OutputWriter.WriteLineAsync("AI-related logs (should be empty):");
        foreach (var log in aiLogs)
            await Context.Current.OutputWriter.WriteLineAsync(log);
        await Assert.That(aiLogs).IsEmpty();
    }

    private static async Task<T> WithTimeout<T>(Task<T> task, string step, int timeoutSeconds = 10)
    {
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));
        if (completed == task)
            return await task;
        throw new TimeoutException($"[TIMEOUT] Step '{step}' timed out after {timeoutSeconds} seconds");
    }
    private static async Task WithTimeout(Task task, string step, int timeoutSeconds = 10)
    {
        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));
        if (completed == task)
        {
            await task;
            return;
        }
        throw new TimeoutException($"[TIMEOUT] Step '{step}' timed out after {timeoutSeconds} seconds");
    }

    private record SessionResponse(Guid SessionId);
    private record TokenResponse(string access_token, int expires_in, string token_type);
} 