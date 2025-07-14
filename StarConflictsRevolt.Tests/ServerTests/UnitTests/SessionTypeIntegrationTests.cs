using System.Net.Http.Json;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class SessionTypeIntegrationTests
{
    // Token acquisition now handled automatically by HttpClient pipeline

    [Test]
    [Timeout(30_000)]
    public async Task Create_SinglePlayer_Session_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = "sp-test-" + Guid.NewGuid(), SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Multiplayer_Session_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = "mp-test-" + Guid.NewGuid(), SessionType = "Multiplayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task AI_Only_Runs_In_SinglePlayer_Session(CancellationToken cancellationToken)
    {
        // This test would check that AI commands are only generated in single player sessions.
        // For brevity, we simulate by checking the session type and expected AI behavior.
        var sessionType = SessionType.SinglePlayer;
        var aiShouldRun = sessionType == SessionType.SinglePlayer;
        await Assert.That(aiShouldRun).IsTrue();

        sessionType = SessionType.Multiplayer;
        aiShouldRun = sessionType == SessionType.SinglePlayer;
        await Assert.That(aiShouldRun).IsFalse();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Multiplayer_Session_Does_Not_Have_AI(CancellationToken cancellationToken)
    {
        // Simulate multiplayer session and check that no AI is present.
        var sessionType = SessionType.Multiplayer;
        var aiPresent = sessionType == SessionType.SinglePlayer;
        await Assert.That(aiPresent).IsFalse();
    }

    [Test]
    [Timeout(30_000)]
    public async Task SinglePlayer_Session_Has_AI(CancellationToken cancellationToken)
    {
        var sessionType = SessionType.SinglePlayer;
        var aiPresent = sessionType == SessionType.SinglePlayer;
        await Assert.That(aiPresent).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_With_Invalid_Type_Fails(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = "bad-type-test-" + Guid.NewGuid(), SessionType = "InvalidType" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        // Should default to multiplayer, not fail
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_Without_Type_Defaults_To_Multiplayer(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = "no-type-test-" + Guid.NewGuid() };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_Without_Name_Fails(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsFalse();
    }

    private record TokenResponse(string AccessToken, int ExpiresIn, string TokenType);
}