using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class GameEngineServerTest
{
    [Test]
    public async Task GameEngineServer_ShouldStartAndRespond()
    {
        // Arrange: Create a test server for the game engine
        using var signalRTestServer = new SignalRTestServer();
        
        // Arrange: Get the web application from the test server
        var app = signalRTestServer.GetWebApplication();
        
        // Fill the database with test data if necessary
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync(); // Ensure the database is created
        
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
        
        
        // Act: Start the application
        await app.StartAsync();
        await hubConnection.StartAsync(); // Start the SignalR connection
        
        await Task.Delay(2000).ConfigureAwait(false);
        
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
        
        // var world = clientWorldStore.GetCurrent();
        // world.Should().NotBeNull("because the world should be initialized and ready for use");
        // world.Galaxy.Should().NotBeNull("because the galaxy should be initialized in the world");
        // world.Galaxy.StarSystems.Should().NotBeEmpty("because the galaxy should contain at least one star system");
        // world.Galaxy.StarSystems.First().Planets.Should().NotBeEmpty("because the first star system should contain at least one planet");
        // world.Galaxy.StarSystems.First().Planets.First().Name.Should().NotBeNullOrEmpty("because the first planet should have a name");
        
        // Gracefully stop the SignalR connection
        await hubConnection.StopAsync();
        
        // Stop the application after tests
        await app.StopAsync();
    }
}