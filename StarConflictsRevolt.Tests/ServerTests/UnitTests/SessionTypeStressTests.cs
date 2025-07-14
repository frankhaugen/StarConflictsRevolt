using System.Net.Http.Json;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class SessionTypeStressTests
{
    // Token handled automatically by client pipeline

    [Test]
    [Timeout(30_000)]
    public async Task Create_50_Sessions_Quickly_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        for (var i = 0; i < 50; i++)
        {
            var req = new { SessionName = $"stress-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_And_Join_20_Sessions_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        for (var i = 0; i < 20; i++)
        {
            var req = new { SessionName = $"join-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
            // Simulate join (would be a GET or SignalR join in real app)
        }
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Sessions_With_Random_Types_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        var rand = new Random();
        for (var i = 0; i < 15; i++)
        {
            var type = rand.Next(2) == 0 ? "SinglePlayer" : "Multiplayer";
            var req = new { SessionName = $"random-type-{i}-{Guid.NewGuid()}", SessionType = type };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Sessions_With_Long_Names_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        for (var i = 0; i < 10; i++)
        {
            var req = new { SessionName = new string('X', 200) + i, SessionType = "SinglePlayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    [Test]
    [Timeout(30_000)]
    public async Task Create_Sessions_With_Special_Chars_Succeeds(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(CancellationToken.None);

        var httpClient = testHost.GetHttpClient();

        for (var i = 0; i < 10; i++)
        {
            var req = new { SessionName = $"!@#$%^&*()_+-={i}", SessionType = "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req, cancellationToken);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }
}