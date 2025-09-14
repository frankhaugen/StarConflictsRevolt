using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using StarConflictsRevolt.Clients.Blazor.Services;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Shared.Communication;
using TUnit;

namespace StarConflictsRevolt.Tests.ClientTests.UnitTests;

/// <summary>
/// Unit tests for GameStateService using bUnit
/// </summary>
public class GameStateServiceTests
{
    private Bunit.TestContext _testContext = null!;
    private IHttpApiClient _mockHttpClient = null!;
    private ISignalRService _mockSignalRService = null!;
    private TelemetryService _telemetryService = null!;
    private GameStateService _gameStateService = null!;

    public GameStateServiceTests()
    {
        _testContext = new Bunit.TestContext();
        
        // Create mocks
        _mockHttpClient = Substitute.For<IHttpApiClient>();
        _mockSignalRService = Substitute.For<ISignalRService>();
        _telemetryService = new TelemetryService();
        
        // Create service under test
        var logger = Substitute.For<ILogger<GameStateService>>();
        _gameStateService = new GameStateService(_mockHttpClient, _mockSignalRService, _telemetryService, logger);
    }
    
    private static StringContent CreateJsonContent<T>(T obj)
    {
        var jsonOptions = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };
        var json = System.Text.Json.JsonSerializer.Serialize(obj, jsonOptions);
        return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    }

    public void Dispose()
    {
        _testContext?.Dispose();
        _telemetryService?.Dispose();
    }

    [Test]
    public async Task CreateSessionAsync_ValidSessionName_ReturnsTrue()
    {
        // Arrange
        var sessionName = "Test Session";
        var sessionResponse = new SessionResponse
        {
            SessionId = Guid.NewGuid(),
            World = new WorldDto(
                Guid.NewGuid(),
                new GalaxyDto(Guid.NewGuid(), new List<StarSystemDto>()),
                null
            )
        };
        
        _mockHttpClient.PostAsync("/game/session", Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = CreateJsonContent(sessionResponse)
            }));

        // Act
        var result = await _gameStateService.CreateSessionAsync(sessionName);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(_gameStateService.CurrentSession).IsNotNull();
        await Assert.That(_gameStateService.CurrentSession!.SessionName).IsEqualTo(sessionName);
        await Assert.That(_gameStateService.CurrentWorld).IsNotNull();
        
        // Verify SignalR was called
        await _mockSignalRService.Received(1).JoinSessionAsync(sessionResponse.SessionId);
    }

    [Test]
    public async Task CreateSessionAsync_HttpClientThrows_ReturnsFalse()
    {
        // Arrange
        var sessionName = "Test Session";
        _mockHttpClient.PostAsync("/game/session", Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<HttpResponseMessage>(new Exception("Network error")));

        // Act
        var result = await _gameStateService.CreateSessionAsync(sessionName);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(_gameStateService.CurrentSession).IsNull();
    }

    [Test]
    public async Task JoinSessionAsync_ValidSessionId_ReturnsTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var sessionResponse = new SessionResponse
        {
            SessionId = sessionId,
            World = new WorldDto(
                Guid.NewGuid(),
                new GalaxyDto(Guid.NewGuid(), new List<StarSystemDto>()),
                null
            )
        };
        
        _mockHttpClient.PostAsync($"/game/session/{sessionId}/join", Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = CreateJsonContent(sessionResponse)
            }));

        // Act
        var result = await _gameStateService.JoinSessionAsync(sessionId);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(_gameStateService.CurrentSession).IsNotNull();
        await Assert.That(_gameStateService.CurrentSession!.Id).IsEqualTo(sessionId);
        
        // Verify SignalR was called
        await _mockSignalRService.Received(1).JoinSessionAsync(sessionId);
    }

    [Test]
    public async Task LeaveSessionAsync_CurrentSessionExists_ReturnsTrue()
    {
        // Arrange - First create a session to set up the state
        var sessionId = Guid.NewGuid();
        var sessionResponse = new SessionResponse
        {
            SessionId = sessionId,
            World = new WorldDto(
                Guid.NewGuid(),
                new GalaxyDto(Guid.NewGuid(), new List<StarSystemDto>()),
                null
            )
        };
        
        _mockHttpClient.PostAsync("/game/session", Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = CreateJsonContent(sessionResponse)
            }));
        
        // Create a session first
        await _gameStateService.CreateSessionAsync("Test Session");

        // Act
        var result = await _gameStateService.LeaveSessionAsync();

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(_gameStateService.CurrentSession).IsNull();
        await Assert.That(_gameStateService.CurrentWorld).IsNull();
        
        // Verify SignalR was called
        await _mockSignalRService.Received(1).StopAsync();
    }

    [Test]
    public async Task GetAvailableSessionsAsync_HttpClientReturnsSessions_ReturnsSessions()
    {
        // Arrange
        var sessionInfos = new List<SessionInfo>
        {
            new SessionInfo
            {
                Id = Guid.NewGuid(),
                SessionName = "Session 1",
                SessionType = "SinglePlayer",
                Created = DateTime.UtcNow
            },
            new SessionInfo
            {
                Id = Guid.NewGuid(),
                SessionName = "Session 2",
                SessionType = "Multiplayer",
                Created = DateTime.UtcNow
            }
        };
        
        _mockHttpClient.GetAsync<List<SessionInfo>>("/game/sessions", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(sessionInfos));

        // Act
        var result = await _gameStateService.GetAvailableSessionsAsync();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result[0].SessionName).IsEqualTo("Session 1");
        await Assert.That(result[1].SessionName).IsEqualTo("Session 2");
    }

    [Test]
    public async Task MoveFleetAsync_ValidParameters_ReturnsTrue()
    {
        // Arrange
        var fleetId = Guid.NewGuid();
        var fromPlanetId = Guid.NewGuid();
        var toPlanetId = Guid.NewGuid();
        
        _mockHttpClient.PostAsync("/game/move-fleet", Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));

        // Act
        var result = await _gameStateService.MoveFleetAsync(fleetId, fromPlanetId, toPlanetId);

        // Assert
        await Assert.That(result).IsTrue();
        await _mockHttpClient.Received(1).PostAsync("/game/move-fleet", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task BuildStructureAsync_ValidParameters_ReturnsTrue()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var structureType = "factory";
        
        _mockHttpClient.PostAsync("/game/build-structure", Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));

        // Act
        var result = await _gameStateService.BuildStructureAsync(planetId, structureType);

        // Assert
        await Assert.That(result).IsTrue();
        await _mockHttpClient.Received(1).PostAsync("/game/build-structure", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task AttackAsync_ValidParameters_ReturnsTrue()
    {
        // Arrange
        var attackerFleetId = Guid.NewGuid();
        var targetFleetId = Guid.NewGuid();
        
        _mockHttpClient.PostAsync("/game/attack", Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)));

        // Act
        var result = await _gameStateService.AttackAsync(attackerFleetId, targetFleetId);

        // Assert
        await Assert.That(result).IsTrue();
        await _mockHttpClient.Received(1).PostAsync("/game/attack", Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task StateChanged_EventFired_InvokesEvent()
    {
        // Arrange
        var eventFired = false;
        _gameStateService.StateChanged += () => eventFired = true;
        
        var sessionResponse = new SessionResponse
        {
            SessionId = Guid.NewGuid(),
            World = new WorldDto(
                Guid.NewGuid(),
                new GalaxyDto(Guid.NewGuid(), new List<StarSystemDto>()),
                null
            )
        };
        
        _mockHttpClient.PostAsync("/game/session", Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = CreateJsonContent(sessionResponse)
            }));

        // Act
        await _gameStateService.CreateSessionAsync("Test Session");

        // Assert
        await Assert.That(eventFired).IsTrue();
    }
}
