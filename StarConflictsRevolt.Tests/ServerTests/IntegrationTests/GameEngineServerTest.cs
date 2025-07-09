using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Server.Datastore;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.IntegrationTests;

public class GameEngineServerTest
{
    private readonly SignalRTestServer _signalRTestServer = new();
    
    [Test]
    public void GameEngineServer_ShouldStartAndRespond()
    {
        // Arrange: Get the web application from the test server
        var app = _signalRTestServer.GetWebApplication();
        
        // Fill the database with test data if necessary
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<GameDbContext>();
        dbContext.Database.EnsureCreated(); // Ensure the database is created
        
        // Listen to SignalR events and persist them in memory for assertions:
        var worldStore = serviceProvider.GetRequiredService<IClientWorldStore>();
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(_signalRTestServer.GetGameServerHubUrl())
            .WithAutomaticReconnect()
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();
        
        hubConnection.On<WorldDto>("FullWorld", worldDto =>
        {
            worldStore.ApplyFull(worldDto);
        });
        
        hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", updates =>
        {
            worldStore.ApplyDeltas(updates);
        });
        
        // Act: Start the application
        app.StartAsync().GetAwaiter().GetResult();
        
        // Stop the application after tests
        app.StopAsync().GetAwaiter().GetResult();
    }
    
        // Example of a test server that starts and responds correctly:
        // // Assert: Check if the application is running and can respond to requests
        // app.Services.GetService(typeof(IEventStore))
        //     .Should().NotBeNull("because the event store should be registered in the service provider");
        //
        // var httpClient = new HttpClient { BaseAddress = new Uri($"{_webApiServer.GetScheme()}://localhost:{_webApiServer.GetPort()}") };
        // var response = httpClient.GetAsync("/").GetAwaiter().GetResult();
        // response.IsSuccessStatusCode.Should().BeTrue("because the root endpoint should respond successfully");
        //
        // Context.Current.OutputWriter.WriteLine(
        //     $"Web API server started at {_webApiServer.GetScheme()}://localhost:{_webApiServer.GetPort()} and responded successfully with status code {response.StatusCode} and content: {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}"
        // );
}