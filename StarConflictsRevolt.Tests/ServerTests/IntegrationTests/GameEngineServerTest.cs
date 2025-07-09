using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Server.Services;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using System.Net.Http.Json;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class GameEngineServerTest
{
    [Test]
    public async Task GameEngineServer_ShouldStartAndRespond()
    {
        // Arrange: Create a test server for the game engine
        using var signalRTestServer = new FullIntegrationTestWebApplicationBuilder();
        
        // Arrange: Get the web application from the test server
        var app = signalRTestServer.Build();
        
        // Fill the database with test data if necessary
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync(); // Ensure the database is created
        
        // Start the application
        await app.StartAsync();
        
        // Create an HttpClient that can communicate with the test server
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{signalRTestServer.GetPort()}") };
        
        // Listen to SignalR events and persist them in memory for assertions:
        var worldStore = serviceProvider.GetRequiredService<IClientWorldStore>();
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(signalRTestServer.GetGameServerHubUrl())
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();
        
        
        var updatesReceived = new List<GameObjectUpdate>();
        
        hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", updatesReceivedList =>
        {
            updatesReceived.AddRange(updatesReceivedList);
            worldStore.ApplyDeltas(updatesReceivedList);
        });
        
        // Ensure the raven event store database is created
        var ravenDbStore = serviceProvider.GetRequiredService<IDocumentStore>();
        ravenDbStore.Initialize(); // Initialize the RavenDB store
        ravenDbStore.Database.Should().NotBeNull("because the RavenDB store should be initialized and ready for use");
        ravenDbStore.Database.Should().NotBeEmpty("because the RavenDB store should have a database created");
        ravenDbStore.Database.Should().Be("StarConflictsRevolt", "because this is the expected database name for the game engine server");
        
        await hubConnection.StartAsync(); // Start the SignalR connection
        
        // Create a test session via HTTP API
        var sessionName = $"test-session-{Guid.NewGuid()}";
        
        var response = await httpClient.PostAsJsonAsync("/game/session", sessionName);
        response.EnsureSuccessStatusCode();
        var sessionObj = await response.Content.ReadFromJsonAsync<SessionResponse>();
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");
        
        await Context.Current.OutputWriter.WriteLineAsync($"Created session: {sessionId}");
        
        // Join the session group
        await hubConnection.SendAsync("JoinWorld", sessionId.ToString());
        
        // Get the world state to find a valid planet ID for sending a command
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

        // Send a command to trigger updates
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
        
        // Wait for updates to be sent
        await Task.Delay(1000).ConfigureAwait(false);
        
        // Assert: Check if the application is running and can respond to requests
        var clientWorldStore = serviceProvider.GetRequiredService<IClientWorldStore>();
        clientWorldStore.Should().NotBeNull("because the client world store should be registered in the service provider");
        
        updatesReceived.Should().NotBeEmpty("because we should have received some updates from the game engine server");
        updatesReceived.Count.Should().BeGreaterThan(0, "because we expect to receive at least one update from the game engine server");
        
        // Write to the test output for debugging purposes:
        await Context.Current.OutputWriter.WriteLineAsync("Received updates from the game engine server:");
        foreach (var update in updatesReceived)
        {
            await Context.Current.OutputWriter.WriteLineAsync($"Update: {update}");
        }
        
        // Gracefully stop the SignalR connection
        await hubConnection.StopAsync();
        
        // Stop the application after tests
        await app.StopAsync();
    }
    
    private record SessionResponse(Guid SessionId);
}