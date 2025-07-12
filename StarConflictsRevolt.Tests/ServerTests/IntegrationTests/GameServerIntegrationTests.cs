using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Raven.Client.Documents.Session;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;
using TUnit.Core;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

[GameServerDataSource]
public partial class GameServerIntegrationTests(GameServerTestHost testHost)
{
    [Test]
    public async Task GameServer_CanStartAndServeRequests()
    {
        // Test that the server is running and can serve requests
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.GetPort()}") };
        
        // Test basic connectivity
        var response = await httpClient.GetAsync("/health");
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task GameServer_CanCreateSessionAndJoinViaSignalR()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.GetPort()}") };

        // Get authentication token
        var tokenResponse = await httpClient.PostAsJsonAsync("/token", new { ClientId = "test-client", Secret = "test-secret" });
        tokenResponse.EnsureSuccessStatusCode();
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenObj!.access_token);

        // Create a session
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionResponse = await httpClient.PostAsJsonAsync("/game/session", new { SessionName = sessionName, SessionType = "Multiplayer" });
        createSessionResponse.EnsureSuccessStatusCode();
        var sessionObj = await createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>();
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");

        // Connect to SignalR and join the session
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(testHost.GetGameServerHubUrl())
            .WithAutomaticReconnect()
            .Build();

        await hubConnection.StartAsync();
        await hubConnection.SendAsync("JoinWorld", sessionId.ToString());

        // Verify we can receive updates
        var receivedUpdates = new List<GameObjectUpdate>();
        hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", updates => receivedUpdates.AddRange(updates));

        // Wait a moment for initial world state
        await Task.Delay(1000);

        await hubConnection.StopAsync();
        await Assert.That(receivedUpdates).IsNotEmpty();
    }

    [Test]
    public async Task GameServer_CanUseRavenDbForPersistence()
    {
        using var session = testHost.CreateSession();
        
        var testEntity = new TestEntity { Name = "Test", Value = 42 };
        await session.StoreAsync(testEntity);
        await session.SaveChangesAsync();

        var loaded = await session.LoadAsync<TestEntity>(testEntity.Id);
        await Assert.That(loaded).IsNotNull();
        await Assert.That(loaded!.Name).IsEqualTo("Test");
        await Assert.That(loaded.Value).IsEqualTo(42);
    }

    private record TokenResponse(string access_token, int expires_in, string token_type);
    private record SessionResponse(Guid SessionId);

    private class TestEntity
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
} 