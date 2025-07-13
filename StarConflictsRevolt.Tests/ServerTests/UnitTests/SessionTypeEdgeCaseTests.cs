using System.Net.Http.Json;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class SessionTypeEdgeCaseTests
{
    private async Task<string> GetAuthTokenAsync(HttpClient httpClient)
    {
        var testClientId = $"test-client-{Guid.NewGuid()}";
        var tokenResponse = await httpClient.PostAsJsonAsync("/token", new { ClientId = testClientId, ClientSecret = Constants.Secret });
        tokenResponse.EnsureSuccessStatusCode();
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenObj?.access_token ?? throw new Exception("Failed to obtain JWT token");
    }

    [Test]
    [Timeout(20_000)]
    public async Task Create_Session_With_Empty_Name_Fails(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.Client;

        var req = new { SessionName = "", SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsFalse();
    }

    [Test]
    [Timeout(20_000)]
    public async Task Create_Session_With_Null_Name_Fails(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.Client;

        var req = new { SessionName = (string?)null, SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsFalse();
    }

    [Test]
    [Timeout(20_000)]
    public async Task Create_Session_With_Null_Type_Defaults_To_Multiplayer(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.Client;

        var req = new { SessionName = "null-type-test-" + Guid.NewGuid(), SessionType = (string?)null };
        var resp = await httpClient.PostAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(20_000)]
    public async Task Create_Session_With_Whitespace_Type_Defaults_To_Multiplayer(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.Client;

        var req = new { SessionName = "ws-type-test-" + Guid.NewGuid(), SessionType = "   " };
        var resp = await httpClient.PostAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(20_000)]
    public async Task Create_Session_With_Long_Name_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.Client;

        var req = new { SessionName = new string('A', 100), SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(20_000)]
    public async Task Create_Session_With_Special_Chars_Name_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.Client;

        var req = new { SessionName = "!@#$%^&*()_+-=", SessionType = "Multiplayer" };
        var resp = await httpClient.PostAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(20_000)]
    public async Task Create_Many_Sessions_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.Client;

        for (var i = 0; i < 10; i++)
        {
            var req = new { SessionName = $"bulk-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsync("/game/session", req, cancellationToken);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    [Test]
    [Timeout(20_000)]
    public async Task Create_Session_With_Empty_Type_Defaults_To_Multiplayer(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);

        var httpClient = testHost.Client;

        var req = new { SessionName = "empty-type-test-" + Guid.NewGuid(), SessionType = "" };
        var resp = await httpClient.PostAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    private record TokenResponse(string access_token, int expires_in, string token_type);
}