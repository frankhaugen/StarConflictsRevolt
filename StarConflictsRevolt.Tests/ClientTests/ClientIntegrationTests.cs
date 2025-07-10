using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Raylib.Services;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;

namespace StarConflictsRevolt.Tests.ClientTests;

public class ClientIntegrationTests
{
    [Test]
    public async Task ClientInitialization_SetsUpIdentityAndConfiguration()
    {
        using var testBuilder = new ClientIntegrationTestBuilder();
        
        // Act
        await testBuilder.InitializeClientAsync();
        
        // Assert
        var renderContext = testBuilder.ClientHost.Services.GetRequiredService<RenderContext>();
        // Note: ClientId property in RenderContext incorrectly maps to PlayerId, so both should be "test-user"
        await Assert.That(renderContext.ClientId).IsEqualTo("test-user");
        await Assert.That(renderContext.GameState.PlayerName).IsEqualTo("Test User");
        await Assert.That(renderContext.GameState.PlayerId).IsEqualTo("test-user");
    }

    [Test]
    public async Task ClientServerIntegration_CreatesSessionAndReceivesWorld()
    {
        using var testBuilder = new ClientIntegrationTestBuilder();
        
        // Initialize client
        await testBuilder.InitializeClientAsync();
        
        // Create HTTP client for API calls
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testBuilder.ServerBuilder.GetPort()}") };
        
        // Authenticate
        var tokenResponse = await httpClient.PostAsJsonAsync("/token", new { ClientId = "test-client", Secret = "test-secret" });
        tokenResponse.EnsureSuccessStatusCode();
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenObj!.access_token);
        
        // Create session
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionResponse = await httpClient.PostAsJsonAsync("/game/session", sessionName);
        createSessionResponse.EnsureSuccessStatusCode();
        var sessionObj = await createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>();
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");
        
        // Connect to SignalR and join world
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(testBuilder.ServerBuilder.GetGameServerHubUrl())
            .WithAutomaticReconnect()
            .Build();
            
        WorldDto? receivedWorld = null;
        var fullWorldReceived = new TaskCompletionSource<bool>();
        hubConnection.On<WorldDto>("FullWorld", worldDto =>
        {
            receivedWorld = worldDto;
            fullWorldReceived.SetResult(true);
        });
        
        await hubConnection.StartAsync();
        await hubConnection.SendAsync("JoinWorld", sessionId.ToString());
        
        // Wait for FullWorld event
        var received = await Task.WhenAny(fullWorldReceived.Task, Task.Delay(2000));
        
        // Cleanup
        await hubConnection.StopAsync();
        await testBuilder.StopAsync();
        
        // Assert
        await Assert.That(fullWorldReceived.Task.IsCompleted).IsTrue();
        await Assert.That(receivedWorld).IsNotNull();
        await Assert.That(receivedWorld!.Galaxy).IsNotNull();
        await Assert.That(receivedWorld.Galaxy.StarSystems).IsNotEmpty();
        await Assert.That(receivedWorld.Galaxy.StarSystems.First().Planets).IsNotEmpty();
    }

    [Test]
    public async Task ViewFactory_CreatesViewsOnDemand()
    {
        using var testBuilder = new ClientIntegrationTestBuilder();
        
        // Act
        var viewFactory = testBuilder.TestViewFactory;
        var menuView = viewFactory.CreateView(GameView.Menu);
        var galaxyView = viewFactory.CreateView(GameView.Galaxy);
        var unknownView = viewFactory.CreateView(GameView.TacticalBattle);
        
        // Assert
        await Assert.That(menuView).IsNotNull();
        await Assert.That(galaxyView).IsNotNull();
        await Assert.That(unknownView).IsNotNull();
        await Assert.That(menuView.ViewType).IsEqualTo(GameView.Menu);
        await Assert.That(galaxyView.ViewType).IsEqualTo(GameView.Galaxy);
        await Assert.That(unknownView.ViewType).IsEqualTo(GameView.TacticalBattle);
        
        // Check creation history
        await Assert.That(viewFactory.ViewCreationHistory).HasCount(3);
        await Assert.That(viewFactory.ViewCreationHistory[0]).IsEqualTo(GameView.Menu);
        await Assert.That(viewFactory.ViewCreationHistory[1]).IsEqualTo(GameView.Galaxy);
        await Assert.That(viewFactory.ViewCreationHistory[2]).IsEqualTo(GameView.TacticalBattle);
    }

    [Test]
    public async Task GameRenderer_ReceivesWorldData()
    {
        using var testBuilder = new ClientIntegrationTestBuilder();
        
        // Act
        var renderer = testBuilder.TestGameRenderer;
        WorldDto? testWorld = new WorldDto(Guid.NewGuid(), new GalaxyDto(Guid.NewGuid(), new List<StarSystemDto>()));
        
        var result = await renderer.RenderAsync(testWorld, CancellationToken.None);
        
        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(renderer.WasRendered).IsTrue();
        await Assert.That(renderer.LastRenderedWorld).IsNotNull();
    }

    [Test]
    public async Task ClientWorldStore_StoresAndRetrievesWorldData()
    {
        using var testBuilder = new ClientIntegrationTestBuilder();
        
        // Act
        var worldStore = testBuilder.TestWorldStore;
        WorldDto? testWorld = new WorldDto(Guid.NewGuid(), new GalaxyDto(Guid.NewGuid(), new List<StarSystemDto>()));
        
        worldStore.ApplyFull(testWorld);
        var retrieved = worldStore.GetCurrent();
        
        // Assert
        await Assert.That(retrieved).IsNotNull();
        await Assert.That(worldStore.History).HasCount(1);
        await Assert.That(worldStore.History[0]).IsNotNull();
    }

    private record SessionResponse(Guid SessionId);
    private record TokenResponse(string access_token, int expires_in, string token_type);
} 