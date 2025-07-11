using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;

namespace StarConflictsRevolt.Tests.ServerTests;

[GameServerDataSource]
public partial class SessionTypeEdgeCaseTests(GameServerTestHost gameServer)
{
    private async Task<string> GetAuthTokenAsync(HttpClient httpClient)
    {
        var testClientId = $"test-client-{Guid.NewGuid()}";
        var tokenResponse = await httpClient.PostAsJsonAsync("/token", new { ClientId = testClientId, Secret = "test-secret" });
        tokenResponse.EnsureSuccessStatusCode();
        var tokenObj = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        return tokenObj?.access_token ?? throw new Exception("Failed to obtain JWT token");
    }

    [Test]
    public async Task Create_Session_With_Empty_Name_Fails()
    {
        // The application is already built and started by GameServerTestHost
        var app = gameServer.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{gameServer.GetPort()}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var req = new { SessionName = "", SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsFalse();
    }

    [Test]
    public async Task Create_Session_With_Null_Name_Fails()
    {
        // The application is already built and started by GameServerTestHost
        var app = gameServer.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{gameServer.GetPort()}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var req = new { SessionName = (string?)null, SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsFalse();
    }

    [Test]
    public async Task Create_Session_With_Null_Type_Defaults_To_Multiplayer()
    {
        // The application is already built and started by GameServerTestHost
        var app = gameServer.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{gameServer.GetPort()}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var req = new { SessionName = "null-type-test-" + Guid.NewGuid(), SessionType = (string?)null };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task Create_Session_With_Whitespace_Type_Defaults_To_Multiplayer()
    {
        // The application is already built and started by GameServerTestHost
        var app = gameServer.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{gameServer.GetPort()}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var req = new { SessionName = "ws-type-test-" + Guid.NewGuid(), SessionType = "   " };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task Create_Session_With_Long_Name_Succeeds()
    {
        // The application is already built and started by GameServerTestHost
        var app = gameServer.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{gameServer.GetPort()}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var req = new { SessionName = new string('A', 100), SessionType = "SinglePlayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task Create_Session_With_Special_Chars_Name_Succeeds()
    {
        // The application is already built and started by GameServerTestHost
        var app = gameServer.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{gameServer.GetPort()}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var req = new { SessionName = "!@#$%^&*()_+-=", SessionType = "Multiplayer" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task Create_Many_Sessions_Succeeds()
    {
        // The application is already built and started by GameServerTestHost
        var app = gameServer.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{gameServer.GetPort()}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        for (int i = 0; i < 10; i++)
        {
            var req = new { SessionName = $"bulk-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    [Test]
    public async Task Create_Session_With_Empty_Type_Defaults_To_Multiplayer()
    {
        // The application is already built and started by GameServerTestHost
        var app = gameServer.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{gameServer.GetPort()}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var req = new { SessionName = "empty-type-test-" + Guid.NewGuid(), SessionType = "" };
        var resp = await httpClient.PostAsJsonAsync("/game/session", req);
        await Assert.That(resp.IsSuccessStatusCode).IsTrue();
    }

    private record TokenResponse(string access_token, int expires_in, string token_type);
} 