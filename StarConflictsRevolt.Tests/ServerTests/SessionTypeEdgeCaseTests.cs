using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TUnit;

namespace StarConflictsRevolt.Tests.ServerTests;

public class SessionTypeEdgeCaseTests
{
    [Test]
    public async Task Create_Session_With_Empty_Name_Fails()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        var req = new { SessionName = "", SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsFalse();
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Session_With_Null_Name_Fails()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        var req = new { SessionName = (string?)null, SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsFalse();
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Session_With_Null_Type_Defaults_To_Multiplayer()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        var req = new { SessionName = "null-type-test-" + Guid.NewGuid(), SessionType = (string?)null };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Session_With_Whitespace_Type_Defaults_To_Multiplayer()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        var req = new { SessionName = "ws-type-test-" + Guid.NewGuid(), SessionType = "   " };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Session_With_Long_Name_Succeeds()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        var req = new { SessionName = new string('A', 100), SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Session_With_Special_Chars_Name_Succeeds()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        var req = new { SessionName = "!@#$%^&*()_+-=", SessionType = "Multiplayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Many_Sessions_Succeeds()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        for (int i = 0; i < 10; i++)
        {
            var req = new { SessionName = $"bulk-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Session_With_Empty_Type_Defaults_To_Multiplayer()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        var req = new { SessionName = "empty-type-test-" + Guid.NewGuid(), SessionType = "" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        
        await app.StopAsync();
    }
} 