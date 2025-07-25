using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using Session = StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay.Session;

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

        // Create some test sessions first
        using var scope = testHost.App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var testSession1 = new Session
        {
            Id = Guid.NewGuid(),
            SessionName = "Test Session 1",
            Created = DateTime.UtcNow.AddMinutes(-10),
            IsActive = true,
            SessionType = SessionType.Multiplayer
        };

        var testSession2 = new Session
        {
            Id = Guid.NewGuid(),
            SessionName = "Test Session 2",
            Created = DateTime.UtcNow.AddMinutes(-5),
            IsActive = true,
            SessionType = SessionType.SinglePlayer
        };

        dbContext.Sessions.Add(testSession1);
        dbContext.Sessions.Add(testSession2);
        await dbContext.SaveChangesAsync(cancellationToken);

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