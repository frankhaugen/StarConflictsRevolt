using System.Text.Json;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;

namespace StarConflictsRevolt.Clients.Raylib;

public class GameCommandService
{
    private readonly HttpApiClient _httpApiClient;
    private readonly GameState _gameState;
    private readonly ILogger<GameCommandService> _logger;
    
    public GameCommandService(GameState gameState, ILogger<GameCommandService> logger, HttpApiClient httpApiClient)
    {
        _gameState = gameState;
        _logger = logger;
        _httpApiClient = httpApiClient;
    }
    
    public async Task<bool> SendCommandAsync(string endpoint, object payload, string? worldId = null)
    {
        try
        {
            var url = worldId != null ? $"{endpoint}?worldId={worldId}" : endpoint;
            var response = await _httpApiClient.PostAsync(url, payload);
            
            if (response.IsSuccessStatusCode)
            {
                _gameState.SetFeedback("Command sent successfully", TimeSpan.FromSeconds(2));
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _gameState.SetFeedback($"Command failed: {response.StatusCode} - {errorContent}", TimeSpan.FromSeconds(5));
                _logger.LogWarning("Command failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _gameState.SetFeedback($"Network error: {ex.Message}", TimeSpan.FromSeconds(5));
            _logger.LogError(ex, "Failed to send command to {Endpoint}", endpoint);
            return false;
        }
    }
    
    public async Task<bool> MoveFleetAsync(Guid fleetId, Guid fromPlanetId, Guid toPlanetId)
    {
        var playerId = !string.IsNullOrEmpty(_gameState.PlayerId) && Guid.TryParse(_gameState.PlayerId, out var pid) ? pid : Guid.Empty;
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
        var playerId = !string.IsNullOrEmpty(_gameState.PlayerId) && Guid.TryParse(_gameState.PlayerId, out var pid) ? pid : Guid.Empty;
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
        var playerId = !string.IsNullOrEmpty(_gameState.PlayerId) && Guid.TryParse(_gameState.PlayerId, out var pid) ? pid : Guid.Empty;
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
        var playerId = !string.IsNullOrEmpty(_gameState.PlayerId) && Guid.TryParse(_gameState.PlayerId, out var pid) ? pid : Guid.Empty;
        var payload = new
        {
            PlayerId = playerId,
            TargetPlayerId = targetPlayerId,
            ProposalType = proposalType,
            Message = message
        };
        
        return await SendCommandAsync("/game/diplomacy", payload, _gameState.Session?.Id.ToString());
    }
    
    public async Task<Guid?> CreateSessionAsync(string sessionName)
    {
        try
        {
            var response = await _httpApiClient.PostAsync("/game/session", sessionName);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                
                if (responseObj.TryGetProperty("sessionId", out var sessionIdElement) && 
                    Guid.TryParse(sessionIdElement.GetString(), out var sessionId))
                {
                    _gameState.SetFeedback("Session created successfully", TimeSpan.FromSeconds(2));
                    return sessionId;
                }
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            _gameState.SetFeedback($"Failed to create session: {response.StatusCode} - {errorContent}", TimeSpan.FromSeconds(5));
            return null;
        }
        catch (Exception ex)
        {
            _gameState.SetFeedback($"Network error: {ex.Message}", TimeSpan.FromSeconds(5));
            _logger.LogError(ex, "Failed to create session");
            return null;
        }
    }
    
    public async Task<WorldDto?> GetWorldStateAsync()
    {
        try
        {
            var world = await _httpApiClient.GetAsync<WorldDto>("/game/state");
            return world;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get world state");
            return null;
        }
    }
} 