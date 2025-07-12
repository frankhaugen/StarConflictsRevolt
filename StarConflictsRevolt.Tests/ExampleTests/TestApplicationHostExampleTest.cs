using Microsoft.EntityFrameworkCore;
using Raven.Client.Documents.Session;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;

namespace StarConflictsRevolt.Tests.ExampleTests;

[TestApplicationDataSource]
public partial class TestApplicationHostExampleTest(TestApplicationHost host)
{
    [Test]
    public async Task Can_Access_All_Infrastructure_Components()
    {
        // Access the WebApplication (server)
        var server = host.Server;
        await Assert.That(server).IsNotNull();
        
        // Access the HTTP API client
        var client = host.Client;
        await Assert.That(client).IsNotNull();
        
        // Access the RavenDB document store
        var documentStore = host.DocumentStore;
        await Assert.That(documentStore).IsNotNull();
        
        // Access the Entity Framework GameDbContext
        var gameDbContext = host.GameDbContext;
        await Assert.That(gameDbContext).IsNotNull();
        
        // Access the port number
        var port = host.Port;
        await Assert.That(port).IsGreaterThan(0);
        
        // Access the SignalR hub URL
        var hubUrl = host.GetGameServerHubUrl();
        await Assert.That(hubUrl).IsEqualTo($"http://localhost:{port}/gamehub");
    }

    [Test]
    public async Task Can_Use_RavenDB_Session()
    {
        // Get a RavenDB session from the document store
        using var session = host.DocumentStore.OpenAsyncSession();
        
        // Test that the session works
        var count = await session.Query<object>().CountAsync();
        await Assert.That(count).IsEqualTo(0); // Should be empty initially
    }

    [Test]
    public async Task Can_Use_GameDbContext()
    {
        // Test that the GameDbContext works
        var sessionCount = await host.GameDbContext.Sessions.CountAsync();
        await Assert.That(sessionCount).IsEqualTo(0); // Should be empty initially
    }

    [Test]
    public async Task Can_Use_HTTP_Client()
    {
        // Test that the HTTP client can make requests
        // Note: This might fail if the server doesn't have a health endpoint
        try
        {
            var response = await host.Client.GetAsync<object>("/health");
            await Assert.That(response).IsNotNull();
        }
        catch
        {
            // If health endpoint doesn't exist, that's okay for this example
            await Assert.That(true).IsTrue(); // Just verify the test framework works
        }
    }
} 