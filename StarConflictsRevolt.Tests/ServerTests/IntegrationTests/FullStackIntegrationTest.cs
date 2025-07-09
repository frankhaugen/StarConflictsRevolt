using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
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
        
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var httpClient = serviceProvider.GetRequiredService<HttpClient>();
        
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
    }

    private record SessionResponse(Guid SessionId);
} 