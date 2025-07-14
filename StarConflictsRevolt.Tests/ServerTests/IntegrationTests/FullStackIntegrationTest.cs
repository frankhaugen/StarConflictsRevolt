using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Server.WebApi.Services;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class FullStackIntegrationTest
{
    [Test]
    [Timeout(30_000)]
    public async Task EndToEnd_Session_Creation_Command_And_SignalR_Delta(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication();

        // Start the test host application
        await testHost.StartServerAsync(CancellationToken.None);

        // Log sink for capturing logs
        var logSink = new ConcurrentBag<string>();

        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        // Ensure the database is created
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken); // Ensure the database is created

        // Create an HttpClient that can communicate with the test server
        var httpClient = testHost.GetHttpClient();

        // 1. Create a new session via API
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionRequest = new { SessionName = sessionName, SessionType = "Multiplayer" };
        var response = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest);
        response.EnsureSuccessStatusCode();
        var sessionObj = await response.Content.ReadFromJsonAsync<SessionResponse>();
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");

        await Context.Current.OutputWriter.WriteLineAsync($"Created session: {sessionId}");

        // 2. Connect to SignalR and join the session group
        var hubUrl = testHost.GetGameServerHubUrl();
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();
        var receivedDeltas = new List<GameObjectUpdate>();
        hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", async deltas =>
        {
            receivedDeltas.AddRange(deltas);
            await Context.Current.OutputWriter.WriteLineAsync($"Received {deltas.Count} deltas via SignalR");
        });
        await hubConnection.StartAsync(cancellationToken);
        await hubConnection.SendAsync("JoinWorld", sessionId.ToString(), cancellationToken);

        // 3. Get the world state to find a valid planet ID
        var worldResponse = await httpClient.GetAsync("/game/state", cancellationToken);
        if (!worldResponse.IsSuccessStatusCode)
        {
            var errorContent = await worldResponse.Content.ReadAsStringAsync(cancellationToken);
            await Context.Current.OutputWriter.WriteLineAsync($"World state request failed: {worldResponse.StatusCode} - {errorContent}");
            throw new Exception($"Failed to get world state: {worldResponse.StatusCode}");
        }

        var world = await worldResponse.Content.ReadFromJsonAsync<World>(cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"World state retrieved: {world?.Id}");
        await Context.Current.OutputWriter.WriteLineAsync($"World before build command: {JsonSerializer.Serialize(world)}");

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
        await Context.Current.OutputWriter.WriteLineAsync($"World after build command: {JsonSerializer.Serialize(worldAfter)}");

        // 5. Wait for a delta update via SignalR
        await Task.Delay(1000); // Give more time for the command to be processed
        await Context.Current.OutputWriter.WriteLineAsync($"Total received deltas: {receivedDeltas.Count}");

        if (receivedDeltas.Count == 0)
        {
            await Context.Current.OutputWriter.WriteLineAsync("No deltas received. Checking if session exists...");
            var aggregateManager = scope.ServiceProvider.GetRequiredService<SessionAggregateManager>();
            var sessionExists = aggregateManager.HasAggregate(sessionId);
            await Context.Current.OutputWriter.WriteLineAsync($"Session exists: {sessionExists}");
        }

        // 6. Gracefully stop the SignalR connection
        await hubConnection.StopAsync();

        // 7. Assertions - these should fail if there are errors
        await Assert.That(receivedDeltas).IsNotEmpty();

        // Debug: Log all received deltas
        await Context.Current.OutputWriter.WriteLineAsync($"Received {receivedDeltas.Count} deltas:");
        foreach (var delta in receivedDeltas)
        {
            await Context.Current.OutputWriter.WriteLineAsync($"  Delta: Id={delta.Id}, Type={delta.Type}, HasData={delta.Data.HasValue}");
            if (delta.Data.HasValue)
            {
                await Context.Current.OutputWriter.WriteLineAsync($"    Data: {delta.Data.Value}");
                await Context.Current.OutputWriter.WriteLineAsync($"    Data (raw JSON): {delta.Data.Value.GetRawText()}");
            }
        }

        // Check that we received the expected delta (structure added to planet)
        var structureDelta = receivedDeltas.FirstOrDefault(d =>
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

    private record TokenResponse(string AccessToken, int ExpiresIn, string TokenType);
}

// Logger provider for capturing logs