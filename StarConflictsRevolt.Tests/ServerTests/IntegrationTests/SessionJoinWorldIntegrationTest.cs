using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using StarConflictsRevolt.Server.WebApi.Datastore;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

[TestHostApplication]
public partial class SessionJoinWorldIntegrationTest(TestHostApplication testHost)
{
    [Test]
    [Timeout(20_000)]
    public async Task SessionCreationAndJoin_SendsFullWorldToJoiningClient(CancellationToken cancellationToken)
    {
        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by TestHostApplication
        var app = testHost.Server;
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };

        // Authenticate
        var testClientId = $"test-client-{Guid.NewGuid()}";
        var tokenResponse = await httpClient.PostAsJsonAsync("/token", new TokenRequest() { ClientId = testClientId, ClientSecret = Constants.Secret }, cancellationToken: cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenObj!.AccessToken);

        // Create session
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionRequest = new { SessionName = sessionName, SessionType = "Multiplayer" };
        var createSessionResponse = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest, cancellationToken: cancellationToken);
        createSessionResponse.EnsureSuccessStatusCode();
        var sessionObj = await createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>(cancellationToken: cancellationToken);
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
} 