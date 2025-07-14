using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class SessionCreationTest
{
    [Test]
    [Timeout(30_000)]
    public async Task SessionCreation_ShouldReturnValidSessionId_WithCamelCaseJson(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // Act - Create a session
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionRequest = new { SessionName = sessionName, SessionType = "SinglePlayer" };
        var response = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest, cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        responseJson.Should().NotBeNullOrEmpty();

        // Test JSON deserialization with camelCase
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var sessionResponse = JsonSerializer.Deserialize<SessionResponse>(responseJson, jsonOptions);
        sessionResponse.Should().NotBeNull();
        sessionResponse!.SessionId.Should().NotBe(Guid.Empty);
        sessionResponse.SessionId.Should().NotBe("00000000-0000-0000-0000-000000000000");

        // Verify the session ID is valid
        sessionResponse.SessionId.ToString().Should().MatchRegex(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");

        // Test that the session can be joined
        var joinRequest = new { PlayerName = "TestPlayer" };
        var joinResponse = await httpClient.PostAsJsonAsync($"/game/session/{sessionResponse.SessionId}/join", joinRequest, cancellationToken);

        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var joinResponseJson = await joinResponse.Content.ReadAsStringAsync(cancellationToken);
        var joinSessionResponse = JsonSerializer.Deserialize<SessionResponse>(joinResponseJson, jsonOptions);
        joinSessionResponse.Should().NotBeNull();
        joinSessionResponse!.SessionId.Should().Be(sessionResponse.SessionId);
    }

    [Test]
    [Timeout(30_000)]
    public async Task SessionCreation_ShouldReturnCamelCaseJsonProperties(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        var httpClient = testHost.GetHttpClient();

        // Act - Create a session
        var sessionName = $"test-session-{Guid.NewGuid()}";
        var createSessionRequest = new { SessionName = sessionName, SessionType = "Multiplayer" };
        var response = await httpClient.PostAsJsonAsync("/game/session", createSessionRequest, cancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        // Verify that the JSON uses camelCase property names
        responseJson.Should().Contain("\"sessionId\"");
        responseJson.Should().Contain("\"world\"");
        responseJson.Should().NotContain("\"SessionId\"");
        responseJson.Should().NotContain("\"World\"");

        // Verify the JSON is valid
        var jsonDocument = JsonDocument.Parse(responseJson);
        jsonDocument.RootElement.TryGetProperty("sessionId", out var sessionIdElement).Should().BeTrue();
        jsonDocument.RootElement.TryGetProperty("world", out var worldElement).Should().BeTrue();

        // Verify sessionId is a valid GUID
        var sessionIdString = sessionIdElement.GetString();
        sessionIdString.Should().NotBeNullOrEmpty();
        Guid.TryParse(sessionIdString, out var sessionId).Should().BeTrue();
        sessionId.Should().NotBe(Guid.Empty);
    }
}