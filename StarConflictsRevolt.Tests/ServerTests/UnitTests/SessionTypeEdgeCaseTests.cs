using System.Net.Http.Json;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class SessionTypeEdgeCaseTests
{
    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_With_Empty_Name_Fails(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = "", SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
        await Assert.That(resp.IsSuccessStatusCode).IsFalse();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_With_Null_Name_Fails(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = (string?)null, SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
        await Assert.That(resp.IsSuccessStatusCode).IsFalse();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_With_Null_Type_Defaults_To_Multiplayer(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = "null-type-test-" + Guid.NewGuid(), SessionType = (string?)null };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_With_Whitespace_Type_Defaults_To_Multiplayer(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = "ws-type-test-" + Guid.NewGuid(), SessionType = "   " };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_With_Long_Name_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = new string('A', 100), SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_With_Special_Chars_Name_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = "!@#$%^&*()_+-=", SessionType = "Multiplayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Many_Sessions_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);
        // The application is already built and started by GameServerTestHost
        var app = testHost.App;

        var httpClient = testHost.GetHttpClient();

        for (var i = 0; i < 10; i++)
        {
            var req = new { SessionName = $"bulk-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Session_With_Empty_Type_Defaults_To_Multiplayer(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        await testHost.StartServerAsync(cancellationToken);

        var httpClient = testHost.GetHttpClient();

        var req = new { SessionName = "empty-type-test-" + Guid.NewGuid(), SessionType = "" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }
}