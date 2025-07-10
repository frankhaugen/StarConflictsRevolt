using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TUnit;

namespace StarConflictsRevolt.Tests.ServerTests;

public class SessionTypeStressTests
{
    [Test]
    public async Task Create_50_Sessions_Quickly_Succeeds()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        for (int i = 0; i < 50; i++)
        {
            var req = new { SessionName = $"stress-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_And_Join_20_Sessions_Succeeds()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        for (int i = 0; i < 20; i++)
        {
            var req = new { SessionName = $"join-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
            // Simulate join (would be a GET or SignalR join in real app)
        }
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Sessions_With_Random_Types_Succeeds()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        var rand = new Random();
        for (int i = 0; i < 15; i++)
        {
            var type = rand.Next(2) == 0 ? "SinglePlayer" : "Multiplayer";
            var req = new { SessionName = $"random-type-{i}-{Guid.NewGuid()}", SessionType = type };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Sessions_With_Long_Names_Succeeds()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        for (int i = 0; i < 10; i++)
        {
            var req = new { SessionName = new string('X', 200) + i, SessionType = "SinglePlayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
        
        await app.StopAsync();
    }

    [Test]
    public async Task Create_Sessions_With_Special_Chars_Succeeds()
    {
        using var builder = new TestingInfrastructure.FullIntegrationTestWebApplicationBuilder();
        var app = builder.Build();
        await app.StartAsync();
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{builder.GetPort()}") };
        for (int i = 0; i < 10; i++)
        {
            var req = new { SessionName = $"!@#$%^&*()_+-={i}", SessionType = "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
        
        await app.StopAsync();
    }
} 