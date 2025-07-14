using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Security;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class SessionJoinWorldIntegrationTest
{
    [Test]
    [Timeout(30_000)]
    public async Task SessionCreationAndJoin_SendsFullWorldToJoiningClient(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by TestHostApplication
        var app = testHost.Server;
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // Create session
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionRequest = new CreateSessionRequest { SessionName = sessionName, SessionType = "Multiplayer" };
        var createSessionResponse = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest, cancellationToken);
        createSessionResponse.EnsureSuccessStatusCode();
        var sessionObj = await createSessionResponse.Content.ReadFromJsonAsync<SessionResponse>(cancellationToken);
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
}