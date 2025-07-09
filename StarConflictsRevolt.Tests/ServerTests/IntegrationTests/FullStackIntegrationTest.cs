using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.Services;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class FullStackIntegrationTest
{
    [Test]
    public async Task EndToEnd_Session_Creation_Command_And_SignalR_Delta()
    {
        using var appBuilderHost = new FullIntegrationTestWebApplicationBuilder();
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

        // 5. Wait for a delta update via SignalR
        await Task.Delay(5000); // Give more time for the command to be processed
        await Context.Current.OutputWriter.WriteLineAsync($"Total received deltas: {_receivedDeltas.Count}");
        
        if (_receivedDeltas.Count == 0)
        {
            await Context.Current.OutputWriter.WriteLineAsync("No deltas received. Checking if session exists...");
            var gameUpdateService = scope.ServiceProvider.GetRequiredService<GameUpdateService>();
            var sessionExists = await gameUpdateService.SessionExistsAsync(sessionId);
            await Context.Current.OutputWriter.WriteLineAsync($"Session exists: {sessionExists}");
        }
        
        await Assert.That(_receivedDeltas).IsNotEmpty();
        
        // 6. Gracefully stop the application and SignalR connection
        await _hubConnection.StopAsync();
        await app.StopAsync();
        await app.DisposeAsync();
    }

    private record SessionResponse(Guid SessionId);
} 