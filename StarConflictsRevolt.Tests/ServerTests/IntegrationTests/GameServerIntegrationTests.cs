using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class GameServerIntegrationTests
{
    [Test]
    [Timeout(20_000)]
    public async Task GameServer_CanStartAndServeRequests(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(cancellationToken);

        // Test that the server is running and can serve requests
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };

        // Test basic connectivity
        var response = await httpClient.GetAsync("/health", cancellationToken);
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameServer_CanCreateSessionAndJoinViaSignalR(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // Get authentication token
        var tokenResponse = await httpClient.PostAsJsonAsync("/token", new TokenRequest { ClientId = "test-client", ClientSecret = Constants.Secret }, cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenObj!.AccessToken);

        // Create a session
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionResponse = await httpClient.PostAsJsonAsync("/game/session", new { SessionName = sessionName, SessionType = "Multiplayer" }, cancellationToken);

        // Output the response for debugging
        await Context.Current.OutputWriter.WriteLineAsync($"Create session request: {createSessionResponse.ReasonPhrase} ({createSessionResponse.StatusCode})");
        await Context.Current.OutputWriter.WriteLineAsync($"Create session response: {await createSessionResponse.Content.ReadAsStringAsync(cancellationToken)}");

        createSessionResponse.EnsureSuccessStatusCode();
        var sessionObj = await createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>(cancellationToken);
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");

        // Connect to SignalR and join the session
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(testHost.GetGameServerHubUrl())
            .WithAutomaticReconnect()
            .Build();

        await hubConnection.StartAsync(cancellationToken);
        await hubConnection.SendAsync("JoinWorld", sessionId.ToString(), cancellationToken);

        // Verify we can receive updates
        var receivedUpdates = new List<GameObjectUpdate>();
        hubConnection.On<List<GameObjectUpdate>>("ReceiveUpdates", updates => receivedUpdates.AddRange(updates));

        // Wait a moment for initial world state
        await Task.Delay(1000, cancellationToken);

        await hubConnection.StopAsync(cancellationToken);
        await Assert.That(receivedUpdates).IsNotEmpty();
    }

    [Test]
    [Timeout(20_000)]
    public async Task GameServer_CanUseRavenDbForPersistence(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(cancellationToken);

        using var session = testHost.DocumentStore.OpenAsyncSession();

        var testEntity = new TestEntity { Name = "Test", Value = 42 };
        await session.StoreAsync(testEntity, cancellationToken);
        await session.SaveChangesAsync(cancellationToken);

        var loaded = await session.LoadAsync<TestEntity>(testEntity.Id, cancellationToken);
        await Assert.That(loaded).IsNotNull();
        await Assert.That(loaded!.Name).IsEqualTo("Test");
        await Assert.That(loaded.Value).IsEqualTo(42);
    }

    private record SessionResponse(Guid SessionId);

    private class TestEntity
    {
        public string Id { get; } = default!;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}