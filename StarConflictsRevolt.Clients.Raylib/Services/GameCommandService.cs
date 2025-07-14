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
        _logger.LogInformation("Sending command to endpoint: {Endpoint}, WorldId: {WorldId}", endpoint, worldId ?? "null");
        _logger.LogDebug("Command payload: {Payload}", JsonSerializer.Serialize(payload));

        try
        {
            var url = worldId != null ? $"{endpoint}?worldId={worldId}" : endpoint;
            _logger.LogDebug("Full URL: {Url}", url);

            var response = await _httpApiClient.PostAsync(url, payload);

            _logger.LogInformation("Command response received. Status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Success response content: {Content}", responseContent);
                _gameState.SetFeedback("Command sent successfully", TimeSpan.FromSeconds(2));
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
        _logger.LogInformation("Moving fleet {FleetId} from planet {FromPlanetId} to planet {ToPlanetId}",
            fleetId, fromPlanetId, toPlanetId);

        var playerId = !string.IsNullOrEmpty(_gameState.PlayerId) && Guid.TryParse(_gameState.PlayerId, out var pid) ? pid : Guid.Empty;
        _logger.LogDebug("Using player ID: {PlayerId}", playerId);

        var payload = new
        {
            PlayerId = playerId,
            FleetId = fleetId,
            FromPlanetId = fromPlanetId,
            ToPlanetId = toPlanetId
        };

        return await SendCommandAsync("/game/move-fleet", payload, _gameState.Session?.Id.ToString());
    }

    public async Task<bool> BuildStructureAsync(Guid planetId, string structureType)
    {
        _logger.LogInformation("Building structure {StructureType} on planet {PlanetId}", structureType, planetId);

        var playerId = !string.IsNullOrEmpty(_gameState.PlayerId) && Guid.TryParse(_gameState.PlayerId, out var pid) ? pid : Guid.Empty;
        _logger.LogDebug("Using player ID: {PlayerId}", playerId);

        var payload = new
        {
            PlayerId = playerId,
            PlanetId = planetId,
            StructureType = structureType
        };

        return await SendCommandAsync("/game/build-structure", payload, _gameState.Session?.Id.ToString());
    }

    public async Task<bool> AttackAsync(Guid attackerFleetId, Guid defenderFleetId, Guid locationPlanetId)
    {
        _logger.LogInformation("Attack initiated. Attacker: {AttackerFleetId}, Defender: {DefenderFleetId}, Location: {LocationPlanetId}",
            attackerFleetId, defenderFleetId, locationPlanetId);

        var playerId = !string.IsNullOrEmpty(_gameState.PlayerId) && Guid.TryParse(_gameState.PlayerId, out var pid) ? pid : Guid.Empty;
        _logger.LogDebug("Using player ID: {PlayerId}", playerId);

        var payload = new
        {
            PlayerId = playerId,
            AttackerFleetId = attackerFleetId,
            DefenderFleetId = defenderFleetId,
            LocationPlanetId = locationPlanetId
        };

        return await SendCommandAsync("/game/attack", payload, _gameState.Session?.Id.ToString());
    }

    public async Task<bool> DiplomacyAsync(Guid targetPlayerId, string proposalType, string message)
    {
        _logger.LogInformation("Diplomacy action. Target: {TargetPlayerId}, Type: {ProposalType}, Message: {Message}",
            targetPlayerId, proposalType, message);

        var playerId = !string.IsNullOrEmpty(_gameState.PlayerId) && Guid.TryParse(_gameState.PlayerId, out var pid) ? pid : Guid.Empty;
        _logger.LogDebug("Using player ID: {PlayerId}", playerId);

        var payload = new
        {
            PlayerId = playerId,
            TargetPlayerId = targetPlayerId,
            ProposalType = proposalType,
            Message = message
        };

        return await SendCommandAsync("/game/diplomacy", payload, _gameState.Session?.Id.ToString());
    }

    public async Task<Guid?> CreateSessionAsync(string sessionName, string sessionType = "Multiplayer")
    {
        _logger.LogInformation("Creating new session with name: {SessionName}, type: {SessionType}", sessionName, sessionType);

        try
        {
            var request = new
            {
                SessionName = sessionName,
                SessionType = sessionType
            };

            var response = await _httpApiClient.PostAsync("/game/session", request);

            _logger.LogInformation("Session creation response received. Status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Session creation response: {Response}", responseJson);

                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

                if (responseObj.TryGetProperty("sessionId", out var sessionIdElement) &&
                    Guid.TryParse(sessionIdElement.GetString(), out var sessionId))
                {
                    _logger.LogInformation("Session created successfully. SessionId: {SessionId}", sessionId);
                    _gameState.SetFeedback("Session created successfully", TimeSpan.FromSeconds(2));
                    return sessionId;
                }

                _logger.LogError("Failed to parse session ID from response: {Response}", responseJson);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to create session. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            _gameState.SetFeedback($"Failed to create session: {response.StatusCode} - {errorContent}", TimeSpan.FromSeconds(5));
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
            var world = await _httpApiClient.GetAsync<WorldDto>("/game/state");

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
            var sessions = await _httpApiClient.GetAsync<List<SessionInfo>>("/game/sessions");

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
            var request = new
            {
                PlayerName = playerName
            };

            var response = await _httpApiClient.PostAsync($"/game/session/{sessionId}/join", request);

            _logger.LogInformation("Join session response received. Status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Join session response: {Response}", responseJson);

                var sessionResponse = JsonSerializer.Deserialize<SessionResponse>(responseJson);
                if (sessionResponse != null)
                {
                    _logger.LogInformation("Successfully joined session {SessionId}", sessionId);
                    _gameState.SetFeedback($"Joined session {sessionResponse.SessionId}", TimeSpan.FromSeconds(2));
                    return sessionResponse;
                }

                _logger.LogError("Failed to parse join session response: {Response}", responseJson);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to join session. Status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
            _gameState.SetFeedback($"Failed to join session: {response.StatusCode} - {errorContent}", TimeSpan.FromSeconds(5));
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

public class SessionInfo
{
    public Guid Id { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public int PlayerCount { get; set; }
}