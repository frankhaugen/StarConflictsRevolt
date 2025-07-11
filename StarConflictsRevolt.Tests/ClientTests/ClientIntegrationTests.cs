using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Json;
using FluentAssertions;

namespace StarConflictsRevolt.Tests.ClientTests;

public class ClientIntegrationTests
{
    [Test]
    public async Task Client_Can_Authenticate_And_CreateSession()
    {
        // Arrange: Set up TestApiHost with required endpoints
        var fakeToken = new TokenResponse
        {
            AccessToken = "fake-jwt",
            TokenType = TokenType.Bearer,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var fakeSessionId = Guid.NewGuid();
        var fakeWorld = new { /* ... fill with minimal world DTO ... */ };

        await using var apiHost = TestApiHost.Create()
            .With(HttpMethod.Get, "/health/game", async ctx => await ctx.Response.WriteAsync("Game on!"))
            .With(HttpMethod.Post, "/token", async ctx =>
            {
                var req = await ctx.Request.ReadFromJsonAsync<TokenRequest>(cancellationToken: ctx.RequestAborted);
                await ctx.Response.WriteAsJsonAsync(fakeToken, cancellationToken: ctx.RequestAborted);
            })
            .With(HttpMethod.Post, "/game/session", async ctx =>
            {
                var resp = new { SessionId = fakeSessionId, World = fakeWorld };
                ctx.Response.StatusCode = 201;
                await ctx.Response.WriteAsJsonAsync(resp, cancellationToken: ctx.RequestAborted);
            })
            .Build(new Uri("http://127.0.0.1:0"));

        await apiHost.ExecuteAsync(async client =>
        {
            // Set up DI for the client, pointing to the TestApiHost
            var services = new ServiceCollection();
            services.AddHttpClient("GameApi", c => c.BaseAddress = client.BaseAddress);
            services.AddTransient<IHttpApiClient>(sp => new HttpApiClient(sp.GetRequiredService<IHttpClientFactory>(), "GameApi"));
            var provider = services.BuildServiceProvider();

            var apiClient = provider.GetRequiredService<IHttpApiClient>();

            // Act: Authenticate
            var tokenResp = await apiClient.PostAsync("/token", new TokenRequest { ClientId = "test", ClientSecret = "test" });
            tokenResp.EnsureSuccessStatusCode();
            var tokenObj = await tokenResp.Content.ReadFromJsonAsync<TokenResponse>();
            await Assert.That(tokenObj).IsNotNull();
            await Assert.That(tokenObj!.AccessToken).IsEqualTo("fake-jwt");

            // Act: Create session
            var sessionResp = await apiClient.PostAsync("/game/session", new { SessionName = "test", SessionType = "Multiplayer" });
            await Assert.That(sessionResp.StatusCode).IsEqualTo(HttpStatusCode.Created);
            var sessionObj = await sessionResp.Content.ReadFromJsonAsync<JsonElement>();
            await Assert.That(sessionObj.GetProperty("sessionId").GetGuid()).IsEqualTo(fakeSessionId);

            // Act: Health check (should always pass)
            var healthResp = await client.GetAsync("/health/game");
            var healthContent = await healthResp.Content.ReadAsStringAsync();
            await Assert.That(healthContent).IsEqualTo("Game on!");
        });
    }
} 