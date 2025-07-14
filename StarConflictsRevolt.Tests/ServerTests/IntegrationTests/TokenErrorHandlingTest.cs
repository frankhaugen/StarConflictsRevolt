using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi.Datastore;
using StarConflictsRevolt.Server.WebApi.Datastore.Entities;
using StarConflictsRevolt.Server.WebApi.Models;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class TokenErrorHandlingTest
{
    [Test]
    [Timeout(30_000)]
    public async Task TokenEndpoint_ShouldReturnDetailedError_WhenSecretIsWrong(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication(true);
        await testHost.StartServerAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // Create some test sessions first
        using var scope = testHost.App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var testSession1 = new StarConflictsRevolt.Server.WebApi.Datastore.Entities.Session
        {
            Id = Guid.NewGuid(),
            SessionName = "Test Session 1",
            Created = DateTime.UtcNow.AddMinutes(-10),
            IsActive = true,
            SessionType = SessionType.Multiplayer
        };

        var testSession2 = new StarConflictsRevolt.Server.WebApi.Datastore.Entities.Session
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
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken: cancellationToken);
        errorResponse.Should().NotBeNull();
        
        // Check error details
        string error = errorResponse!.error;
        string errorDescription = errorResponse.error_description;
        string clientId = errorResponse.client_id;
        int sessionCount = errorResponse.session_count;
        
        error.Should().Be("invalid_client");
        errorDescription.Should().Be("Invalid client secret. Please check your configuration.");
        clientId.Should().Be("test-client");
        sessionCount.Should().Be(2);
        
        // Check that existing sessions are included
        var existingSessions = errorResponse.existing_sessions as Newtonsoft.Json.Linq.JArray;
        existingSessions.Should().NotBeNull();
        existingSessions!.Count.Should().Be(2);
        
        // Verify session details
        var firstSession = existingSessions[0];
        var sessionName = firstSession["sessionName"]?.ToString();
        sessionName.Should().BeOneOf("Test Session 1", "Test Session 2");
        firstSession["sessionType"].Should().NotBeNull();
    }

    [Test]
    [Timeout(30_000)]
    public async Task TokenEndpoint_ShouldReturnDetailedError_WhenRequestIsInvalid(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication(true);
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
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken: cancellationToken);
        errorResponse.Should().NotBeNull();
        
        string error = errorResponse!.error;
        string errorDescription = errorResponse.error_description;
        
        error.Should().Be("invalid_request");
        errorDescription.Should().Be("ClientId and ClientSecret are required.");
    }

    [Test]
    [Timeout(30_000)]
    public async Task TokenEndpoint_ShouldReturnDetailedError_WhenDatabaseIsUnavailable(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication(false); // Don't start database
        await testHost.StartServerAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // Act - Try to get token with correct secret but no database
        var tokenRequest = new TokenRequest
        {
            ClientId = "test-client",
            ClientSecret = "SuperSecretKeyForJwtTokenGeneration123" // Correct secret
        };

        var response = await httpClient.PostAsJsonAsync("/token", tokenRequest, cancellationToken);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.ServiceUnavailable);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken: cancellationToken);
        errorResponse.Should().NotBeNull();
        
        string error = errorResponse!.error;
        string errorDescription = errorResponse.error_description;
        
        error.Should().Be("service_unavailable");
        errorDescription.Should().Contain("Database");
    }
} 