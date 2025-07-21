using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ClientTests;

public class ClientIntegrationTests
{
    [Test]
    public async Task HealthCheck_ReturnsGameOn()
    {
        await using var apiHost = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/game", async ctx => await ctx.Response.WriteAsync("Game on!"))
            .Build(new Uri("http://127.0.0.1:0"));

        await apiHost.ExecuteAsync(async client =>
        {
            var resp = await client.GetAsync("/health/game");
            var content = await resp.Content.ReadAsStringAsync();
            await Assert.That(content).IsEqualTo("Game on!");
        });
    }

    [Test]
    public async Task HealthCheck_Failure_ReturnsServerError()
    {
        await using var apiHost = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/broken", async ctx =>
            {
                ctx.Response.StatusCode = 500;
                await ctx.Response.WriteAsync("Server error");
            })
            .Build(new Uri("http://127.0.0.1:0"));

        await apiHost.ExecuteAsync(async client =>
        {
            var resp = await client.GetAsync("/health/broken");
            await Assert.That(resp.StatusCode).IsEqualTo(HttpStatusCode.InternalServerError);
        });
    }

    [Test]
    public async Task TokenEndpoint_Success_ReturnsToken()
    {
        var fakeToken = new TokenResponse
        {
            AccessToken = "fake-jwt",
            TokenType = TokenType.Bearer,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        await using var apiHost = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/game", async ctx => await ctx.Response.WriteAsync("Game on!"))
            .With(HttpMethod.Post, "/token", async ctx =>
            {
                var req = await ctx.Request.ReadFromJsonAsync<TokenRequest>(ctx.RequestAborted);
                await ctx.Response.WriteAsJsonAsync(fakeToken, ctx.RequestAborted);
            })
            .Build(new Uri("http://127.0.0.1:0"));

        await apiHost.ExecuteAsync(async client =>
        {
            var services = new ServiceCollection();
            services.AddHttpClient("GameApi", c => c.BaseAddress = client.BaseAddress);
            services.AddTransient<IHttpApiClient>(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), "GameApi"));
            var provider = services.BuildServiceProvider();
            var apiClient = provider.GetRequiredService<IHttpApiClient>();

            var tokenResp = await apiClient.PostAsync("/token", new TokenRequest { ClientId = "test", ClientSecret = "test" });
            tokenResp.EnsureSuccessStatusCode();
            var tokenObj = await tokenResp.Content.ReadFromJsonAsync<TokenResponse>();
            await Assert.That(tokenObj).IsNotNull();
            await Assert.That(tokenObj!.AccessToken).IsEqualTo("fake-jwt");
        });
    }

    [Test]
    public async Task TokenEndpoint_Failure_ReturnsUnauthorized()
    {
        await using var apiHost = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/game", async ctx => await ctx.Response.WriteAsync("Game on!"))
            .With(HttpMethod.Post, "/token", async ctx =>
            {
                var req = await ctx.Request.ReadFromJsonAsync<TokenRequest>(ctx.RequestAborted);
                if (req?.ClientSecret == "fail")
                {
                    ctx.Response.StatusCode = 401;
                    await ctx.Response.WriteAsync("Invalid credentials");
                }
                else
                {
                    await ctx.Response.WriteAsJsonAsync(new TokenResponse
                    {
                        AccessToken = "fake-jwt",
                        TokenType = TokenType.Bearer,
                        ExpiresAt = DateTime.UtcNow.AddHours(1)
                    }, ctx.RequestAborted);
                }
            })
            .Build(new Uri("http://127.0.0.1:0"));

        await apiHost.ExecuteAsync(async client =>
        {
            var services = new ServiceCollection();
            services.AddHttpClient("GameApi", c => c.BaseAddress = client.BaseAddress);
            services.AddTransient<IHttpApiClient>(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), "GameApi"));
            var provider = services.BuildServiceProvider();
            var apiClient = provider.GetRequiredService<IHttpApiClient>();

            var failTokenResp = await apiClient.PostAsync("/token", new TokenRequest { ClientId = "test", ClientSecret = "fail" });
            await Assert.That(failTokenResp.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        });
    }

    [Test]
    public async Task SessionCreation_Success_ReturnsSessionIdAndType()
    {
        var fakeSessionId = Guid.NewGuid();
        var fakeWorld = new { Name = "TestWorld" };
        await using var apiHost = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/game", async ctx => await ctx.Response.WriteAsync("Game on!"))
            .With(HttpMethod.Post, "/game/session", async ctx =>
            {
                var req = await ctx.Request.ReadFromJsonAsync<JsonElement>(ctx.RequestAborted);
                var sessionType = req.TryGetProperty("SessionType", out var st) ? st.GetString() : "Multiplayer";
                var resp = new { sessionId = fakeSessionId, world = fakeWorld, sessionType };
                ctx.Response.StatusCode = 201;
                await ctx.Response.WriteAsJsonAsync(resp, ctx.RequestAborted);
            })
            .Build(new Uri("http://127.0.0.1:0"));

        await apiHost.ExecuteAsync(async client =>
        {
            var services = new ServiceCollection();
            services.AddHttpClient("GameApi", c => c.BaseAddress = client.BaseAddress);
            services.AddTransient<IHttpApiClient>(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), "GameApi"));
            var provider = services.BuildServiceProvider();
            var apiClient = provider.GetRequiredService<IHttpApiClient>();

            var sessionResp = await apiClient.PostAsync("/game/session", new { SessionName = "test", SessionType = "Multiplayer" });
            await Assert.That(sessionResp.StatusCode).IsEqualTo(HttpStatusCode.Created);
            var sessionObj = await sessionResp.Content.ReadFromJsonAsync<JsonElement>();
            await Assert.That(sessionObj.GetProperty("sessionId").GetGuid()).IsEqualTo(fakeSessionId);
            await Assert.That(sessionObj.GetProperty("sessionType").GetString()).IsEqualTo("Multiplayer");
        });
    }

    [Test]
    public async Task SessionCreation_Failure_ReturnsBadRequest()
    {
        await using var apiHost = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/game", async ctx => await ctx.Response.WriteAsync("Game on!"))
            .With(HttpMethod.Post, "/game/session/fail", async ctx =>
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.WriteAsync("Bad session request");
            })
            .Build(new Uri("http://127.0.0.1:0"));

        await apiHost.ExecuteAsync(async client =>
        {
            var services = new ServiceCollection();
            services.AddHttpClient("GameApi", c => c.BaseAddress = client.BaseAddress);
            services.AddTransient<IHttpApiClient>(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), "GameApi"));
            var provider = services.BuildServiceProvider();
            var apiClient = provider.GetRequiredService<IHttpApiClient>();

            var failSessionResp = await apiClient.PostAsync("/game/session/fail", new { });
            await Assert.That(failSessionResp.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        });
    }

    [Test]
    public async Task SessionGet_ReturnsSession()
    {
        var fakeSessionId = Guid.NewGuid();
        var fakeWorld = new { Name = "TestWorld" };
        await using var apiHost = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/game", async ctx => await ctx.Response.WriteAsync("Game on!"))
            .With(HttpMethod.Get, "/game/session", async ctx =>
            {
                var resp = new { sessionId = fakeSessionId, world = fakeWorld };
                await ctx.Response.WriteAsJsonAsync(resp, ctx.RequestAborted);
            })
            .Build(new Uri("http://127.0.0.1:0"));

        await apiHost.ExecuteAsync(async client =>
        {
            var services = new ServiceCollection();
            services.AddHttpClient("GameApi", c => c.BaseAddress = client.BaseAddress);
            services.AddTransient<IHttpApiClient>(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), "GameApi"));
            var provider = services.BuildServiceProvider();
            var apiClient = provider.GetRequiredService<IHttpApiClient>();

            var getSessionResp = await apiClient.GetAsync<JsonElement>("/game/session");
            await Assert.That(getSessionResp.ValueKind).IsNotEqualTo(JsonValueKind.Undefined);
            await Assert.That(getSessionResp!.GetProperty("sessionId").GetGuid()).IsEqualTo(fakeSessionId);
        });
    }
}