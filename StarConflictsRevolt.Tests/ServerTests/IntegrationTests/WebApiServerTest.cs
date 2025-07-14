using System.Net.Http.Json;
using FluentAssertions;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class WebApiServerTest
{
    [Test]
    [Timeout(30_000)]
    public async Task WebApiServer_ShouldStartAndRespond(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        // Start the server
        await testHost.StartServerAsync(cancellationToken);

        // The application is already built and started by TestHostApplication
        var app = testHost.Server;

        // Assert: Check if the application is running and can respond to requests
        app.Services.GetService(typeof(IEventStore))
            .Should().NotBeNull("because the event store should be registered in the service provider");

        var httpClient = testHost.GetHttpClient();
        var response = await httpClient.GetAsync("/", cancellationToken);
        response.IsSuccessStatusCode.Should().BeTrue("because the root endpoint should respond successfully");

        await Context.Current.OutputWriter.WriteLineAsync(
            $"Web API server started at http://localhost:{testHost.Port} and responded successfully with status code {response.StatusCode} and content: {await response.Content.ReadAsStringAsync(cancellationToken)}"
        );
    }

    [Test]
    [Timeout(30_000)]
    public async Task TokenEndpoint_ShouldWorkWithoutAuthentication(CancellationToken cancellationToken)
    {
        var testHost = new TestHostApplication(false);

        // Start the server
        await testHost.StartServerAsync(cancellationToken);

        // Create a raw HttpClient (not the configured one) to test the token endpoint
        var rawHttpClient = new HttpClient { BaseAddress = new Uri($"http://localhost:{testHost.Port}") };

        // Test the token endpoint directly
        var tokenRequest = new { ClientId = "test-client", ClientSecret = Constants.Secret };
        var tokenResponse = await rawHttpClient.PostAsJsonAsync("/token", tokenRequest, cancellationToken);

        tokenResponse.IsSuccessStatusCode.Should().BeTrue("because the token endpoint should work without authentication");

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        await Context.Current.OutputWriter.WriteLineAsync($"Token response: {tokenContent}");

        tokenContent.Should().Contain("access_token", "because the response should contain an access token");
    }
}