using System.Text.Json;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Services;

public class GameCommandService
{
    private readonly GameState _gameState;
    private readonly IHttpApiClient _httpApiClient;
    private readonly ILogger<GameCommandService> _logger;

    public GameCommandService(GameState gameState, ILogger<GameCommandService> logger, IHttpApiClient httpApiClient)
    {
        _gameState = gameState;
        _logger = logger;
        _httpApiClient = httpApiClient;
        _logger.LogInformation("GameCommandService initialized");
    }

    public async Task<bool> SendCommandAsync(string endpoint, object payload, string? worldId = null)
    {
        _logger.LogInformation("Sending command to endpoint: {Endpoint}", endpoint);

        try
        {
            var uri = worldId != null ? $"{endpoint}?worldId={worldId}" : endpoint;
            var response = await _httpApiClient.PostAsync(uri, payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Command sent successfully to {Endpoint}", endpoint);
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Command failed. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            _gameState.SetFeedback($"Command failed: {response.StatusCode} - {errorContent}", TimeSpan.FromSeconds(5));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending command to {Endpoint}", endpoint);
            _gameState.SetFeedback($"Network error: {ex.Message}", TimeSpan.FromSeconds(5));
            return false;
        }
    }

    public async Task<bool> MoveFleetAsync(Guid fleetId, Guid fromPlanetId, Guid toPlanetId)
    {
        _logger.LogInformation("Move fleet initiated. Fleet: {FleetId}, From: {FromPlanetId}, To: {ToPlanetId}",
            fleetId, fromPlanetId, toPlanetId);

        var worldId = _gameState.Session?.Id;
        return await _httpApiClient.MoveFleetAsync(fleetId, fromPlanetId, toPlanetId, worldId);
    }

    public async Task<bool> BuildStructureAsync(Guid planetId, string structureType)
    {
        _logger.LogInformation("Build structure initiated. Planet: {PlanetId}, Structure: {StructureType}",
            planetId, structureType);

        var worldId = _gameState.Session?.Id;
        return await _httpApiClient.BuildStructureAsync(planetId, structureType, worldId);
    }

    public async Task<bool> AttackAsync(Guid attackerFleetId, Guid defenderFleetId, Guid locationPlanetId)
    {
        _logger.LogInformation("Attack initiated. Attacker: {AttackerFleetId}, Defender: {DefenderFleetId}, Location: {LocationPlanetId}",
            attackerFleetId, defenderFleetId, locationPlanetId);

        var worldId = _gameState.Session?.Id;
        return await _httpApiClient.AttackAsync(attackerFleetId, defenderFleetId, locationPlanetId, worldId);
    }

    public async Task<bool> DiplomacyAsync(Guid targetPlayerId, string proposalType, string message)
    {
        _logger.LogInformation("Diplomacy action. Target: {TargetPlayerId}, Type: {ProposalType}, Message: {Message}",
            targetPlayerId, proposalType, message);

        var worldId = _gameState.Session?.Id;
        return await _httpApiClient.DiplomacyAsync(targetPlayerId, proposalType, message, worldId);
    }

    public async Task<SessionResponse?> CreateSessionAsync(string sessionName, string sessionType = "Multiplayer")
    {
        _logger.LogInformation("Creating new session with name: {SessionName}, type: {SessionType}", sessionName, sessionType);

        try
        {
            var sessionResponse = await _httpApiClient.CreateNewSessionAsync(sessionName, sessionType);
            
            if (sessionResponse != null)
            {
                _logger.LogInformation("Session created successfully. SessionId: {SessionId}", sessionResponse.SessionId);
                _gameState.SetFeedback("Session created successfully", TimeSpan.FromSeconds(2));
                return sessionResponse;
            }

            _logger.LogWarning("Failed to create session - no response received");
            _gameState.SetFeedback("Failed to create session", TimeSpan.FromSeconds(5));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while creating session");
            _gameState.SetFeedback($"Network error: {ex.Message}", TimeSpan.FromSeconds(5));
            return null;
        }
    }

    public async Task<WorldDto?> GetWorldStateAsync()
    {
        _logger.LogInformation("Requesting world state from server");

        try
        {
            var world = await _httpApiClient.GetWorldStateAsync();

            if (world != null)
            {
                _logger.LogInformation("World state retrieved successfully. WorldId: {WorldId}", world.Id);
                _logger.LogDebug("World contains {StarSystemCount} star systems",
                    world.Galaxy?.StarSystems?.Count() ?? 0);
            }
            else
            {
                _logger.LogWarning("World state request returned null");
            }

            return world;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting world state");
            return null;
        }
    }

    public async Task<List<SessionInfo>?> ListSessionsAsync()
    {
        _logger.LogInformation("Requesting list of available sessions");

        try
        {
            var sessions = await _httpApiClient.GetSessionsAsync();

            if (sessions != null)
            {
                _logger.LogInformation("Retrieved {SessionCount} available sessions", sessions.Count);
                foreach (var session in sessions)
                {
                    _logger.LogDebug("Session: {SessionId} - {SessionName} ({SessionType})", 
                        session.Id, session.SessionName, session.SessionType);
                }
            }
            else
            {
                _logger.LogWarning("Session list request returned null");
            }

            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while getting session list");
            return null;
        }
    }

    public async Task<SessionResponse?> JoinSessionAsync(Guid sessionId, string playerName)
    {
        _logger.LogInformation("Joining session {SessionId} as player {PlayerName}", sessionId, playerName);

        try
        {
            var sessionResponse = await _httpApiClient.JoinSessionAsync(sessionId, playerName);
            
            if (sessionResponse != null)
            {
                _logger.LogInformation("Successfully joined session {SessionId}", sessionId);
                _gameState.SetFeedback($"Joined session {sessionResponse.SessionId}", TimeSpan.FromSeconds(2));
                return sessionResponse;
            }

            _logger.LogWarning("Failed to join session - no response received");
            _gameState.SetFeedback("Failed to join session", TimeSpan.FromSeconds(5));
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while joining session");
            _gameState.SetFeedback($"Network error: {ex.Message}", TimeSpan.FromSeconds(5));
            return null;
        }
    }
}