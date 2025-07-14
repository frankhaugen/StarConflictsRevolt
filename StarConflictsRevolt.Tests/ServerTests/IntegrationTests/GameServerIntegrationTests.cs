using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class GameServerIntegrationTests
{
    [Test]
    [Timeout(30_000)]
    public async Task GameServer_CanStartAndServeRequests(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);
        await testHost.StartServerAsync(cancellationToken);

        // Test that the server is running and can serve requests
        var httpClient = testHost.GetHttpClient();

        // Test basic connectivity
        var response = await httpClient.GetAsync("/health", cancellationToken);
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    [Timeout(30_000)]
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


    private class TestEntity
    {
        public string Id { get; } = default!;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}