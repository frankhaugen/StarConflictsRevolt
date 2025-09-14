using Bunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StarConflictsRevolt.Clients.Blazor.Services;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Shared.Communication;

namespace StarConflictsRevolt.Tests.ClientTests.UnitTests;

/// <summary>
/// Unit tests for GameStateService using bUnit
/// </summary>
public class GameStateServiceTests
{
    private TestContext _testContext = null!;
    private IHttpApiClient _mockHttpClient = null!;
    private ISignalRService _mockSignalRService = null!;
    private TelemetryService _telemetryService = null!;
    private GameStateService _gameStateService = null!;

    [SetUp]
    public void Setup()
    {
        _testContext = new TestContext();
        
        // Create mocks
        _mockHttpClient = Substitute.For<IHttpApiClient>();
        _mockSignalRService = Substitute.For<ISignalRService>();
        _telemetryService = new TelemetryService();
        
        // Create service under test
        _gameStateService = new GameStateService(_mockHttpClient, _mockSignalRService, _telemetryService);
    }

    [TearDown]
    public void TearDown()
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
            World = new WorldDto
            {
                Galaxy = new GalaxyDto
                {
                    StarSystems = new List<StarSystemDto>()
                }
            }
        };
        
        _mockHttpClient.CreateNewSessionAsync(sessionName, "SinglePlayer")
            .Returns(sessionResponse);

        // Act
        var result = await _gameStateService.CreateSessionAsync(sessionName);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_gameStateService.CurrentSession, Is.Not.Null);
        Assert.That(_gameStateService.CurrentSession!.SessionName, Is.EqualTo(sessionName));
        Assert.That(_gameStateService.CurrentWorld, Is.Not.Null);
        
        // Verify SignalR was called
        await _mockSignalRService.Received(1).JoinSessionAsync(sessionResponse.SessionId);
    }

    [Test]
    public async Task CreateSessionAsync_HttpClientThrows_ReturnsFalse()
    {
        // Arrange
        var sessionName = "Test Session";
        _mockHttpClient.CreateNewSessionAsync(sessionName, "SinglePlayer")
            .Throws(new Exception("Network error"));

        // Act
        var result = await _gameStateService.CreateSessionAsync(sessionName);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(_gameStateService.CurrentSession, Is.Null);
    }

    [Test]
    public async Task JoinSessionAsync_ValidSessionId_ReturnsTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var sessionResponse = new SessionResponse
        {
            SessionId = sessionId,
            World = new WorldDto
            {
                Galaxy = new GalaxyDto
                {
                    StarSystems = new List<StarSystemDto>()
                }
            }
        };
        
        _mockHttpClient.JoinSessionAsync(sessionId, "Player")
            .Returns(sessionResponse);

        // Act
        var result = await _gameStateService.JoinSessionAsync(sessionId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_gameStateService.CurrentSession, Is.Not.Null);
        Assert.That(_gameStateService.CurrentSession!.Id, Is.EqualTo(sessionId));
        
        // Verify SignalR was called
        await _mockSignalRService.Received(1).JoinSessionAsync(sessionId);
    }

    [Test]
    public async Task LeaveSessionAsync_CurrentSessionExists_ReturnsTrue()
    {
        // Arrange
        _gameStateService.CurrentSession = new SessionDto
        {
            Id = Guid.NewGuid(),
            SessionName = "Test Session",
            SessionType = "SinglePlayer",
            Created = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        var result = await _gameStateService.LeaveSessionAsync();

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_gameStateService.CurrentSession, Is.Null);
        Assert.That(_gameStateService.CurrentWorld, Is.Null);
        
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
        
        _mockHttpClient.GetSessionsAsync().Returns(sessionInfos);

        // Act
        var result = await _gameStateService.GetAvailableSessionsAsync();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[0].SessionName, Is.EqualTo("Session 1"));
        Assert.That(result[1].SessionName, Is.EqualTo("Session 2"));
    }

    [Test]
    public async Task MoveFleetAsync_ValidParameters_ReturnsTrue()
    {
        // Arrange
        var fleetId = Guid.NewGuid();
        var fromPlanetId = Guid.NewGuid();
        var toPlanetId = Guid.NewGuid();
        
        _mockHttpClient.MoveFleetAsync(fleetId, fromPlanetId, toPlanetId)
            .Returns(true);

        // Act
        var result = await _gameStateService.MoveFleetAsync(fleetId, fromPlanetId, toPlanetId);

        // Assert
        Assert.That(result, Is.True);
        await _mockHttpClient.Received(1).MoveFleetAsync(fleetId, fromPlanetId, toPlanetId);
    }

    [Test]
    public async Task BuildStructureAsync_ValidParameters_ReturnsTrue()
    {
        // Arrange
        var planetId = Guid.NewGuid();
        var structureType = "factory";
        
        _mockHttpClient.BuildStructureAsync(planetId, structureType)
            .Returns(true);

        // Act
        var result = await _gameStateService.BuildStructureAsync(planetId, structureType);

        // Assert
        Assert.That(result, Is.True);
        await _mockHttpClient.Received(1).BuildStructureAsync(planetId, structureType);
    }

    [Test]
    public async Task AttackAsync_ValidParameters_ReturnsTrue()
    {
        // Arrange
        var attackerFleetId = Guid.NewGuid();
        var targetFleetId = Guid.NewGuid();
        
        _mockHttpClient.AttackAsync(attackerFleetId, targetFleetId, Guid.Empty)
            .Returns(true);

        // Act
        var result = await _gameStateService.AttackAsync(attackerFleetId, targetFleetId);

        // Assert
        Assert.That(result, Is.True);
        await _mockHttpClient.Received(1).AttackAsync(attackerFleetId, targetFleetId, Guid.Empty);
    }

    [Test]
    public void StateChanged_EventFired_InvokesEvent()
    {
        // Arrange
        var eventFired = false;
        _gameStateService.StateChanged += () => eventFired = true;

        // Act
        _gameStateService.CurrentSession = new SessionDto
        {
            Id = Guid.NewGuid(),
            SessionName = "Test Session",
            SessionType = "SinglePlayer",
            Created = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        Assert.That(eventFired, Is.True);
    }
}
