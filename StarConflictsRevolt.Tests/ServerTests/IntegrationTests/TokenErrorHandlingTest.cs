using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class TokenErrorHandlingTest
{
    [Test]
    [Timeout(30_000)]
    public async Task TokenEndpoint_ShouldReturnDetailedError_WhenSecretIsWrong(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // Create some test sessions via persistence
        await testHost.UseGamePersistenceAsync(async persistence =>
        {
            await persistence.CreateSessionAsync("Test Session 1", SessionType.Multiplayer, null, cancellationToken);
            await persistence.CreateSessionAsync("Test Session 2", SessionType.SinglePlayer, null, cancellationToken);
        });

        // Act - Try to get token with wrong secret
        var tokenRequest = new TokenRequest
        {
            ClientId = "test-client",
            ClientSecret = "wrong-secret"
        };

        var response = await httpClient.PostAsJsonAsync("/token", tokenRequest, cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        errorResponse.Should().NotBeNull();

        // Check error details
        errorResponse.Should().Contain("Invalid client secret");
    }

    [Test]
    [Timeout(30_000)]
    public async Task TokenEndpoint_ShouldReturnDetailedError_WhenRequestIsInvalid(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // Act - Try to get token with missing fields
        var tokenRequest = new TokenRequest
        {
            ClientId = "", // Empty client ID
            ClientSecret = "some-secret"
        };

        var response = await httpClient.PostAsJsonAsync("/token", tokenRequest, cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        errorResponse.Should().NotBeNull();

        // Check error details
        errorResponse.Should().Contain("ClientId and ClientSecret are required");
    }
}