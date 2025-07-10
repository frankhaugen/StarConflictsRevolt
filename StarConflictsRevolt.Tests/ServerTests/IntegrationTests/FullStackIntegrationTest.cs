using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.Services;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using System.Collections.Concurrent;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class FullStackIntegrationTest
{
    [Test]
    public async Task EndToEnd_Session_Creation_Command_And_SignalR_Delta()
    {
        // Log sink for capturing logs
        var logSink = new ConcurrentBag<string>();

        using var appBuilderHost = new FullIntegrationTestWebApplicationBuilder();
        
        // Add our log provider to capture all logs from the application
        appBuilderHost.LoggingBuilder.AddProvider(new TestLoggerProvider(logSink));
        
        var app = appBuilderHost.WebApplication;
        
        // Ensure the database is created
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync(); // Ensure the database is created
        
        await app.StartAsync();
        
        // Create an HttpClient that can communicate with the test server
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{appBuilderHost.GetPort()}") };
        
        // 1. Create a new session via API
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var response = await httpClient.PostAsJsonAsync("/game/session", sessionName);
        response.EnsureSuccessStatusCode();
        var sessionObj = await response.Content.ReadFromJsonAsync<SessionResponse>();
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");
        
        await Context.Current.OutputWriter.WriteLineAsync($"Created session: {sessionId}");

        // 2. Connect to SignalR and join the session group
        var hubUrl = appBuilderHost.GetGameServerHubUrl();
        var _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();
        var _receivedDeltas = new List<GameObjectUpdate>();
        _hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", async deltas => 
        {
            _receivedDeltas.AddRange(deltas);
            await Context.Current.OutputWriter.WriteLineAsync($"Received {deltas.Count} deltas via SignalR");
        });
        await _hubConnection.StartAsync();
        await _hubConnection.SendAsync("JoinWorld", sessionId.ToString());

        // 3. Get the world state to find a valid planet ID
        var worldResponse = await httpClient.GetAsync("/game/state");
        if (!worldResponse.IsSuccessStatusCode)
        {
            var errorContent = await worldResponse.Content.ReadAsStringAsync();
            await Context.Current.OutputWriter.WriteLineAsync($"World state request failed: {worldResponse.StatusCode} - {errorContent}");
            throw new Exception($"Failed to get world state: {worldResponse.StatusCode}");
        }
        
        var world = await worldResponse.Content.ReadFromJsonAsync<World>();
        await Context.Current.OutputWriter.WriteLineAsync($"World state retrieved: {world?.Id}");
        await Context.Current.OutputWriter.WriteLineAsync($"World before build command: {System.Text.Json.JsonSerializer.Serialize(world)}");
        
        if (world?.Galaxy?.StarSystems?.FirstOrDefault()?.Planets?.FirstOrDefault() is not Planet planet)
        {
            await Context.Current.OutputWriter.WriteLineAsync("No planet found in the world");
            throw new Exception("No planet found in the world");
        }
        
        await Context.Current.OutputWriter.WriteLineAsync($"Found planet: {planet.Id} - {planet.Name}");

        // 4. Send a command (e.g., build structure) via API using the actual planet ID
        var buildCommand = new
        {
            PlayerId = Guid.NewGuid(),
            PlanetId = planet.Id,
            StructureType = "Mine"
        };
        
        await Context.Current.OutputWriter.WriteLineAsync($"Sending build command for planet: {planet.Id}");
        var buildResponse = await httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", buildCommand);
        
        if (!buildResponse.IsSuccessStatusCode)
        {
            var errorContent = await buildResponse.Content.ReadAsStringAsync();
            await Context.Current.OutputWriter.WriteLineAsync($"Build command failed: {buildResponse.StatusCode} - {errorContent}");
            throw new Exception($"Build command failed: {buildResponse.StatusCode} - {errorContent}");
        }
        
        await Context.Current.OutputWriter.WriteLineAsync("Build command sent successfully");

        // 4.5. Get the world state after the build command
        var worldAfterResponse = await httpClient.GetAsync("/game/state");
        var worldAfter = await worldAfterResponse.Content.ReadFromJsonAsync<World>();
        await Context.Current.OutputWriter.WriteLineAsync($"World after build command: {System.Text.Json.JsonSerializer.Serialize(worldAfter)}");

        // 5. Wait for a delta update via SignalR
        await Task.Delay(1000); // Give more time for the command to be processed
        await Context.Current.OutputWriter.WriteLineAsync($"Total received deltas: {_receivedDeltas.Count}");
        
        if (_receivedDeltas.Count == 0)
        {
            await Context.Current.OutputWriter.WriteLineAsync("No deltas received. Checking if session exists...");
            var sessionManagerService = scope.ServiceProvider.GetRequiredService<SessionManagerService>();
            var sessionExists = await sessionManagerService.SessionExistsAsync(sessionId);
            await Context.Current.OutputWriter.WriteLineAsync($"Session exists: {sessionExists}");
        }
        
        // 6. Gracefully stop the application and SignalR connection
        await _hubConnection.StopAsync();
        await app.StopAsync();
        await app.DisposeAsync();
        
        // 7. Assertions - these should fail if there are errors
        await Assert.That(_receivedDeltas).IsNotEmpty();
        
        // Debug: Log all received deltas
        await Context.Current.OutputWriter.WriteLineAsync($"Received {_receivedDeltas.Count} deltas:");
        foreach (var delta in _receivedDeltas)
        {
            await Context.Current.OutputWriter.WriteLineAsync($"  Delta: Id={delta.Id}, Type={delta.Type}, HasData={delta.Data.HasValue}");
            if (delta.Data.HasValue)
            {
                await Context.Current.OutputWriter.WriteLineAsync($"    Data: {delta.Data.Value}");
                await Context.Current.OutputWriter.WriteLineAsync($"    Data (raw JSON): {delta.Data.Value.GetRawText()}");
            }
        }
        
        // Check that we received the expected delta (structure added to planet)
        var structureDelta = _receivedDeltas.FirstOrDefault(d =>
            (d.Type == UpdateType.Added || d.Type == UpdateType.Changed) &&
            (
                (d.Data.HasValue && d.Data.Value.TryGetProperty("variant", out var variant) && variant.GetString() == "mine") ||
                (d.Data.HasValue && d.Data.Value.TryGetProperty("structureType", out var structureType) && structureType.GetString() == "Mine")
            )
        );
        await Assert.That(structureDelta).IsNotNull();
        // Verify the structure was added to the correct planet
        if (structureDelta?.Data.HasValue == true)
        {
            var structureData = structureDelta.Data.Value;
            if (structureData.TryGetProperty("variant", out var variant))
                await Assert.That(variant.GetString()).IsEqualTo("mine");
            else if (structureData.TryGetProperty("structureType", out var structureType))
                await Assert.That(structureType.GetString()).IsEqualTo("Mine");
            else
                Assert.Fail("Delta did not contain expected variant or structureType property");
        }

        // 8. Assert no critical errors/warnings in logs
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

// Logger provider for capturing logs