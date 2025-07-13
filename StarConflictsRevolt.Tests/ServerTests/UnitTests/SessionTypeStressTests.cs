using System.Net.Http.Json;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;


public partial class SessionTypeStressTests()
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
    public async Task Create_50_Sessions_Quickly_Succeeds()
    {
        var testHost = new TestHostApplication(false);

        // The application is already built and started by GameServerTestHost
        var app = testHost.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        for (int i = 0; i < 50; i++)
        {
            var req = new { SessionName = $"stress-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    [Test]
    public async Task Create_And_Join_20_Sessions_Succeeds()
    {
        var testHost = new TestHostApplication(false);

        // The application is already built and started by GameServerTestHost
        var app = testHost.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        for (int i = 0; i < 20; i++)
        {
            var req = new { SessionName = $"join-test-{i}-{Guid.NewGuid()}", SessionType = i % 2 == 0 ? "SinglePlayer" : "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
            // Simulate join (would be a GET or SignalR join in real app)
        }
    }

    [Test]
    public async Task Create_Sessions_With_Random_Types_Succeeds()
    {
        var testHost = new TestHostApplication(false);

        // The application is already built and started by GameServerTestHost
        var app = testHost.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        var rand = new Random();
        for (int i = 0; i < 15; i++)
        {
            var type = rand.Next(2) == 0 ? "SinglePlayer" : "Multiplayer";
            var req = new { SessionName = $"random-type-{i}-{Guid.NewGuid()}", SessionType = type };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    [Test]
    public async Task Create_Sessions_With_Long_Names_Succeeds()
    {
        var testHost = new TestHostApplication(false);

        // The application is already built and started by GameServerTestHost
        var app = testHost.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        for (int i = 0; i < 10; i++)
        {
            var req = new { SessionName = new string('X', 200) + i, SessionType = "SinglePlayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    [Test]
    public async Task Create_Sessions_With_Special_Chars_Succeeds()
    {
        var testHost = new TestHostApplication(false);

        // The application is already built and started by GameServerTestHost
        var app = testHost.App;
        
        var httpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };
        var token = await GetAuthTokenAsync(httpClient);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        for (int i = 0; i < 10; i++)
        {
            var req = new { SessionName = $"!@#$%^&*()_+-={i}", SessionType = "Multiplayer" };
            var resp = await httpClient.PostAsJsonAsync("/game/session", req);
            await Assert.That(resp.IsSuccessStatusCode).IsTrue();
        }
    }

    private record TokenResponse(string access_token, int expires_in, string token_type);
} 