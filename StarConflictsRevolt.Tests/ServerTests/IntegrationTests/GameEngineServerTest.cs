using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Clients.Raylib.Game.World;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Planets;
using StarConflictsRevolt.Server.WebApi.Core.Domain.World;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class GameEngineServerTest
{
    [Test]
    [Timeout(30_000)]
    public async Task GameEngineServer_ShouldStartAndRespond(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        // Fill the database with test data if necessary
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var dbContext = serviceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken); // Ensure the database is created

        // === AUTHENTICATION: Obtain JWT token ===
        var testClientId = $"test-client-{Guid.NewGuid()}";
        var tokenResponse = await httpClient.PostAsJsonAsync("/token", new TokenRequest { ClientId = testClientId, ClientSecret = Constants.Secret }, cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        if (tokenObj == null || string.IsNullOrEmpty(tokenObj.AccessToken))
            throw new Exception("Failed to obtain JWT token for test user");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenObj.AccessToken);
        // === END AUTHENTICATION ===

        // Listen to SignalR events and persist them in memory for assertions:
        var worldStore = serviceProvider.GetRequiredService<IClientWorldStore>();
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(testHost.GetGameServerHubUrl())
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
        ravenDbStore.Database.Should().MatchRegex("^(StarConflictsRevolt|test-database-).*$", "because test runs may use isolated test-database-* names");

        await hubConnection.StartAsync(cancellationToken); // Start the SignalR connection

        // Create a test session via HTTP API
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionRequest = new { SessionName = sessionName, SessionType = "Multiplayer" };
        var response = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest);
        response.EnsureSuccessStatusCode();
        var sessionObj = await response.Content.ReadFromJsonAsync<SessionResponse>();
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");

        await Context.Current.OutputWriter.WriteLineAsync($"Created session: {sessionId}");

        // Join the session group
        await hubConnection.SendAsync("JoinWorld", sessionId.ToString());

        // Get the world state to find a valid planet ID for sending a command
        var worldResponse = await httpClient.GetAsync("/game/state", cancellationToken);
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
        var buildResponse = await httpClient.PostAsJsonAsync($"/game/build-structure?worldId={sessionId}", buildCommand, cancellationToken: cancellationToken);

        if (!buildResponse.IsSuccessStatusCode)
        {
            var errorContent = await buildResponse.Content.ReadAsStringAsync();
            await Context.Current.OutputWriter.WriteLineAsync($"Build command failed: {buildResponse.StatusCode} - {errorContent}");
            throw new Exception($"Build command failed: {buildResponse.StatusCode} - {errorContent}");
        }

        await Context.Current.OutputWriter.WriteLineAsync("Build command sent successfully");

        // Wait for updates to be sent
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

        // Assert: Check if the application is running and can respond to requests
        var clientWorldStore = serviceProvider.GetRequiredService<IClientWorldStore>();
        clientWorldStore.Should().NotBeNull("because the client world store should be registered in the service provider");

        updatesReceived.Should().NotBeEmpty("because we should have received some updates from the game engine server");
        updatesReceived.Count.Should().BeGreaterThan(0, "because we expect to receive at least one update from the game engine server");

        // Write to the test output for debugging purposes:
        await Context.Current.OutputWriter.WriteLineAsync("Received updates from the game engine server:");
        foreach (var update in updatesReceived) await Context.Current.OutputWriter.WriteLineAsync($"Update: {update}");

        // Gracefully stop the SignalR connection
        await hubConnection.StopAsync(cancellationToken);
    }
}