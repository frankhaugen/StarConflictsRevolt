using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.Datastore;
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
        var sessionId = sessionObj?.SessionId.ToString() ?? throw new Exception("No sessionId returned");

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
        _hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", deltas => _receivedDeltas.AddRange(deltas));
        await _hubConnection.StartAsync();
        await _hubConnection.SendAsync("JoinWorld", sessionId);

        // 3. Send a command (e.g., build structure) via API
        var buildCommand = new
        {
            PlayerId = Guid.NewGuid(),
            PlanetId = Guid.NewGuid(),
            StructureType = "Mine"
        };
        var buildResponse = await httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", buildCommand);
        buildResponse.EnsureSuccessStatusCode();

        // 4. Wait for a delta update via SignalR
        await Assert.That(_receivedDeltas).IsNotEmpty();
        
        // 5. Gracefully stop the application and SignalR connection
        await _hubConnection.StopAsync();
        await app.StopAsync();
        await app.DisposeAsync();
    }

    private record SessionResponse(Guid SessionId);
} 