using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Shared.Communication;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Clients.Blazor.Services;

public class GameStateService : IGameStateService
{
    private readonly IHttpApiClient _httpClient;
    private readonly ISignalRService _signalRService;
    private readonly TelemetryService _telemetryService;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<GameStateService> _logger;
    private WorldDto? _currentWorld;
    private SessionDto? _currentSession;
    private bool _isConnected = false;
    private DateTime _lastConnectionCheck = DateTime.MinValue;
    private readonly TimeSpan _connectionCheckInterval = TimeSpan.FromSeconds(30);

    public GameStateService(IHttpApiClient httpClient, ISignalRService signalRService, TelemetryService telemetryService, ILogger<GameStateService> logger)
    {
        _httpClient = httpClient;
        _signalRService = signalRService;
        _telemetryService = telemetryService;
        _logger = logger;
        _activitySource = new ActivitySource("StarConflictsRevolt.Blazor");
        
        // Subscribe to SignalR updates
        _signalRService.FullWorldReceived += OnWorldUpdated;
        _signalRService.UpdatesReceived += OnSignalRUpdates;
        _signalRService.ConnectionClosed += OnSignalRConnectionClosed;
        _signalRService.Reconnected += OnSignalRReconnected;
        
        _logger.LogInformation("GameStateService initialized");
    }

    public WorldDto? CurrentWorld => _currentWorld;
    public SessionDto? CurrentSession => _currentSession;
    public bool IsConnected => _isConnected;

    public event Action? StateChanged;

    public async Task<bool> CreateSessionAsync(string sessionName)
    {
        using var activity = _activitySource.StartActivity("CreateSession");
        activity?.SetTag("session.name", sessionName);
        activity?.SetTag("session.type", "SinglePlayer");
        
        _logger.LogInformation("Creating new session: {SessionName}", sessionName);
        
        try
        {
            // Check connection status before making request
            await CheckConnectionStatusAsync();
            
            _telemetryService.RecordHttpRequest();
            var stopwatch = Stopwatch.StartNew();
            
            _logger.LogDebug("Sending create session request to server");
            var sessionResponse = await _httpClient.CreateNewSessionAsync(sessionName, "SinglePlayer");
            
            stopwatch.Stop();
            _telemetryService.RecordHttpResponseTime(stopwatch.Elapsed.TotalSeconds);
            
            if (sessionResponse != null)
            {
                _logger.LogInformation("Successfully created session {SessionId} with name {SessionName}", 
                    sessionResponse.SessionId, sessionName);
                
                _currentSession = new SessionDto
                {
                    Id = sessionResponse.SessionId,
                    SessionName = sessionName,
                    SessionType = "SinglePlayer",
                    Created = DateTime.UtcNow,
                    IsActive = true
                };
                _currentWorld = sessionResponse.World;
                
                _logger.LogDebug("Joining SignalR session {SessionId}", sessionResponse.SessionId);
                await _signalRService.JoinSessionAsync(sessionResponse.SessionId);
                
                _isConnected = true;
                NotifyStateChanged();
                
                _telemetryService.RecordGameAction("create_session");
                activity?.SetStatus(ActivityStatusCode.Ok);
                return true;
            }
            else
            {
                _logger.LogWarning("Server returned null response for session creation");
            }
        }
        catch (Exception ex)
        {
            _telemetryService.RecordHttpError();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error creating session {SessionName}: {ErrorMessage}", sessionName, ex.Message);
        }
        return false;
    }

    public async Task<bool> JoinSessionAsync(Guid sessionId)
    {
        _logger.LogInformation("Joining session: {SessionId}", sessionId);
        
        try
        {
            await CheckConnectionStatusAsync();
            
            var sessionResponse = await _httpClient.JoinSessionAsync(sessionId, "Player");
            if (sessionResponse != null)
            {
                _logger.LogInformation("Successfully joined session {SessionId}", sessionId);
                
                _currentSession = new SessionDto
                {
                    Id = sessionResponse.SessionId,
                    SessionName = "Joined Session",
                    SessionType = "Multiplayer",
                    Created = DateTime.UtcNow,
                    IsActive = true
                };
                _currentWorld = sessionResponse.World;
                
                _logger.LogDebug("Joining SignalR session {SessionId}", sessionId);
                await _signalRService.JoinSessionAsync(sessionId);
                
                _isConnected = true;
                NotifyStateChanged();
                return true;
            }
            else
            {
                _logger.LogWarning("Server returned null response for session join {SessionId}", sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining session {SessionId}: {ErrorMessage}", sessionId, ex.Message);
        }
        return false;
    }

    public async Task<bool> LeaveSessionAsync()
    {
        try
        {
            await _signalRService.StopAsync();
            _currentSession = null;
            _currentWorld = null;
            NotifyStateChanged();
            return true;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error leaving session: {ex.Message}");
        }
        return false;
    }

    public async Task<List<SessionDto>> GetAvailableSessionsAsync()
    {
        try
        {
            var sessionInfos = await _httpClient.GetSessionsAsync();
            if (sessionInfos != null)
            {
                return sessionInfos.Select(si => new SessionDto
                {
                    Id = si.Id,
                    SessionName = si.SessionName,
                    SessionType = si.SessionType,
                    Created = si.Created,
                    IsActive = true
                }).ToList();
            }
            return new List<SessionDto>();
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error getting sessions: {ex.Message}");
            return new List<SessionDto>();
        }
    }

    public async Task<bool> MoveFleetAsync(Guid fleetId, Guid fromPlanetId, Guid toPlanetId)
    {
        try
        {
            var result = await _httpClient.MoveFleetAsync(fleetId, fromPlanetId, toPlanetId);
            return result;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error moving fleet: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> BuildStructureAsync(Guid planetId, string structureType)
    {
        try
        {
            var result = await _httpClient.BuildStructureAsync(planetId, structureType);
            return result;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error building structure: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> AttackAsync(Guid attackerFleetId, Guid targetFleetId)
    {
        try
        {
            // TODO: Need location planet ID for attack
            var result = await _httpClient.AttackAsync(attackerFleetId, targetFleetId, Guid.Empty);
            return result;
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error attacking: {ex.Message}");
            return false;
        }
    }

    private void OnWorldUpdated(WorldDto world)
    {
        _currentWorld = world;
        NotifyStateChanged();
    }

    private void OnSignalRUpdates(List<GameObjectUpdate> updates)
    {
        _telemetryService.RecordSignalRMessage();
        // Process updates as needed
    }

    private async Task CheckConnectionStatusAsync()
    {
        // Only check if enough time has passed since last check
        if (DateTime.UtcNow - _lastConnectionCheck < _connectionCheckInterval)
        {
            return;
        }

        try
        {
            _logger.LogDebug("Checking connection status with server");
            var isHealthy = await _httpClient.IsHealthyAsync();
            
            if (isHealthy != _isConnected)
            {
                _isConnected = isHealthy;
                _logger.LogInformation("Connection status changed to: {IsConnected}", _isConnected);
                NotifyStateChanged();
            }
            
            _lastConnectionCheck = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check connection status");
            if (_isConnected)
            {
                _isConnected = false;
                NotifyStateChanged();
            }
        }
    }

    private void OnSignalRConnectionClosed(Exception? exception)
    {
        _logger.LogWarning("SignalR connection closed: {Exception}", exception?.Message ?? "Unknown reason");
        if (_isConnected)
        {
            _isConnected = false;
            NotifyStateChanged();
        }
    }

    private void OnSignalRReconnected(string connectionId)
    {
        _logger.LogInformation("SignalR reconnected with connection ID: {ConnectionId}", connectionId);
        if (!_isConnected)
        {
            _isConnected = true;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged()
    {
        _logger.LogDebug("Notifying state change. Connected: {IsConnected}, Session: {SessionId}", 
            _isConnected, _currentSession?.Id);
        StateChanged?.Invoke();
    }
}
