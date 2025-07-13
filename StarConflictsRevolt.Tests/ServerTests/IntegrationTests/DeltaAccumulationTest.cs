using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using System.Collections.Concurrent;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Models;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

[TestHostApplication]
public partial class DeltaAccumulationTest(TestHostApplication testHost)
{
    [Test]
    [Timeout(60_000)] // Increased timeout for more complex interactions
    public async Task Should_Not_Accumulate_Deltas_Repeatedly(CancellationToken cancellationToken)
    {
        await testHost.StartServerAsync(cancellationToken);
        // Log sink for capturing logs
        var logSink = new ConcurrentBag<string>();

        // The application is already built and started by GameServerTestHost
        var app = testHost.App;
        
        // Ensure the database is created
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        
        // Create an HttpClient that can communicate with the test server
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };

        // === AUTHENTICATION: Obtain JWT token ===
        var testClientId = $"test-client-{Guid.NewGuid()}";
        var tokenResponse = await httpClient.PostAsJsonAsync("/token", new { ClientId = testClientId, ClientSecret = Constants.Secret });
        tokenResponse.EnsureSuccessStatusCode();
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
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
        await Context.Current.OutputWriter.WriteLineAsync($"Mariell creating session: {sessionName}");
        
        var createSessionResponse = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest);
        createSessionResponse.EnsureSuccessStatusCode();
        var sessionObj = await createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>();
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");
        
        await Context.Current.OutputWriter.WriteLineAsync($"Mariell created session: {sessionId}");

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
        
        var mariellDeltaCounts = new List<int>();
        mariellHubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", async deltas => 
        {
            mariellDeltaCounts.Add(deltas.Count);
            await Context.Current.OutputWriter.WriteLineAsync($"Mariell received {deltas.Count} deltas via SignalR");
        });
        
        await mariellHubConnection.StartAsync();
        await mariellHubConnection.SendAsync("JoinWorld", sessionId.ToString());
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
        
        var frankDeltaCounts = new List<int>();
        frankHubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", async deltas => 
        {
            frankDeltaCounts.Add(deltas.Count);
            await Context.Current.OutputWriter.WriteLineAsync($"Frank received {deltas.Count} deltas via SignalR");
        });
        
        await frankHubConnection.StartAsync();
        await frankHubConnection.SendAsync("JoinWorld", sessionId.ToString());
        await Context.Current.OutputWriter.WriteLineAsync("Frank joined the session");

        // 4. Wait a moment for both players to receive initial world state
        await Task.Delay(1000);
        
        await Context.Current.OutputWriter.WriteLineAsync($"Mariell received {mariellDeltaCounts.Count} delta batches");
        await Context.Current.OutputWriter.WriteLineAsync($"Frank received {frankDeltaCounts.Count} delta batches");

        // 5. Get the world state to find a valid planet ID for testing
        var worldResponse = await httpClient.GetAsync("/game/state");
        if (!worldResponse.IsSuccessStatusCode)
        {
            var errorContent = await worldResponse.Content.ReadAsStringAsync();
            await Context.Current.OutputWriter.WriteLineAsync($"World state request failed: {worldResponse.StatusCode} - {errorContent}");
            throw new Exception($"Failed to get world state: {worldResponse.StatusCode}");
        }
        
        var world = await worldResponse.Content.ReadFromJsonAsync<World>();
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
        var mariellBuildResponse = await httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", mariellBuildCommand);
        
        if (!mariellBuildResponse.IsSuccessStatusCode)
        {
            var errorContent = await mariellBuildResponse.Content.ReadAsStringAsync();
            await Context.Current.OutputWriter.WriteLineAsync($"Mariell's build command failed: {mariellBuildResponse.StatusCode} - {errorContent}");
            throw new Exception($"Mariell's build command failed: {mariellBuildResponse.StatusCode} - {errorContent}");
        }
        
        await Context.Current.OutputWriter.WriteLineAsync("Mariell's build command sent successfully");

        // 7. Wait for both players to receive the update
        await Task.Delay(2000);
        
        await Context.Current.OutputWriter.WriteLineAsync($"After Mariell's command - Mariell received {mariellDeltaCounts.Count} delta batches");
        await Context.Current.OutputWriter.WriteLineAsync($"After Mariell's command - Frank received {frankDeltaCounts.Count} delta batches");

        // 8. Frank sends a different build command
        var frankBuildCommand = new
        {
            PlayerId = playerFrankId,
            PlanetId = planet.Id,
            StructureType = "Shipyard"
        };
        
        await Context.Current.OutputWriter.WriteLineAsync($"Frank sending build command for planet: {planet.Id}");
        var frankBuildResponse = await httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", frankBuildCommand);
        
        if (!frankBuildResponse.IsSuccessStatusCode)
        {
            var errorContent = await frankBuildResponse.Content.ReadAsStringAsync();
            await Context.Current.OutputWriter.WriteLineAsync($"Frank's build command failed: {frankBuildResponse.StatusCode} - {errorContent}");
            throw new Exception($"Frank's build command failed: {frankBuildResponse.StatusCode} - {errorContent}");
        }
        
        await Context.Current.OutputWriter.WriteLineAsync("Frank's build command sent successfully");

        // 9. Wait for both players to receive the update
        await Task.Delay(2000);
        
        await Context.Current.OutputWriter.WriteLineAsync($"After Frank's command - Mariell received {mariellDeltaCounts.Count} delta batches");
        await Context.Current.OutputWriter.WriteLineAsync($"After Frank's command - Frank received {frankDeltaCounts.Count} delta batches");

        // 10. Gracefully stop the SignalR connections
        await mariellHubConnection.StopAsync();
        await frankHubConnection.StopAsync();
        
        // 11. Analyze the delta patterns
        await Context.Current.OutputWriter.WriteLineAsync("=== DELTA ANALYSIS ===");
        await Context.Current.OutputWriter.WriteLineAsync($"Mariell received {mariellDeltaCounts.Count} total delta batches");
        await Context.Current.OutputWriter.WriteLineAsync($"Frank received {frankDeltaCounts.Count} total delta batches");
        
        // Check for repeated delta counts (indicating accumulation)
        var mariellRepeatedCounts = mariellDeltaCounts.GroupBy(x => x).Where(g => g.Count() > 1).ToList();
        var frankRepeatedCounts = frankDeltaCounts.GroupBy(x => x).Where(g => g.Count() > 1).ToList();
        
        await Context.Current.OutputWriter.WriteLineAsync($"Mariell repeated delta counts: {string.Join(", ", mariellRepeatedCounts.Select(g => $"{g.Key}({g.Count()})"))}");
        await Context.Current.OutputWriter.WriteLineAsync($"Frank repeated delta counts: {string.Join(", ", frankRepeatedCounts.Select(g => $"{g.Key}({g.Count()})"))}");
        
        // Assertions
        await Assert.That(mariellDeltaCounts).IsNotEmpty();
        await Assert.That(frankDeltaCounts).IsNotEmpty();
        
        // Both players should have received the same number of delta batches
        await Assert.That(mariellDeltaCounts.Count).IsEqualTo(frankDeltaCounts.Count);
        
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
    private record TokenResponse(string access_token, int expires_in, string token_type);
} 