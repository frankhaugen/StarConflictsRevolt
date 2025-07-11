using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;
using StarConflictsRevolt.Server.WebApi.Datastore;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

[TestHostApplication]
public partial class SessionJoinWorldIntegrationTest(TestHostApplication testHost, CancellationToken cancellationToken)
{
    [Test]
    [Timeout(20)]
    public async Task SessionCreationAndJoin_SendsFullWorldToJoiningClient(CancellationToken cancellationToken)
    {
        // The application is already built and started by TestHostApplication
        var app = testHost.Server;
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };

        // Authenticate
        var testClientId = $"test-client-{Guid.NewGuid()}";
        var tokenResponse = await httpClient.PostAsJsonAsync("/token", new { ClientId = testClientId, Secret = "test-secret" });
        tokenResponse.EnsureSuccessStatusCode();
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenObj!.access_token);

        // Create session
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionRequest = new { SessionName = sessionName, SessionType = "Multiplayer" };
        var createSessionResponse = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest);
        createSessionResponse.EnsureSuccessStatusCode();
        var sessionObj = await createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>();
        var sessionId = sessionObj?.SessionId ?? throw new Exception("No sessionId returned");

        // Connect to SignalR and join world
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(testHost.GetGameServerHubUrl())
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
        await hubConnection.StopAsync();
        await Assert.That(fullWorldReceived.Task.IsCompleted).IsTrue();
        await Assert.That(receivedWorld).IsNotNull();
        await Assert.That(receivedWorld!.Galaxy).IsNotNull();
        await Assert.That(receivedWorld.Galaxy.StarSystems).IsNotEmpty();
        await Assert.That(receivedWorld.Galaxy.StarSystems.First().Planets).IsNotEmpty();
    }

    private record SessionResponse(Guid SessionId);
    private record TokenResponse(string access_token, int expires_in, string token_type);
} 