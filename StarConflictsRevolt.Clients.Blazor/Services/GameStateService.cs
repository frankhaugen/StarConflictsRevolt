using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Clients.Shared.Authentication;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Shared.Communication;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace StarConflictsRevolt.Clients.Blazor.Services;

public class GameStateService : IGameStateService
{
    private const string DefaultPlayerName = "Player";

    private readonly IHttpApiClient _httpClient;
    private readonly ISignalRService _signalRService;
    private readonly TelemetryService _telemetryService;
    private readonly IClientIdProvider _clientIdProvider;
    private readonly IClientSessionStorage _sessionStorage;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<GameStateService> _logger;
    private WorldDto? _currentWorld;
    private SessionDto? _currentSession;
    private Guid? _currentPlayerId;
    private bool _isConnected = false;
    private DateTime _lastConnectionCheck = DateTime.MinValue;
    private readonly TimeSpan _connectionCheckInterval = TimeSpan.FromSeconds(30);

    private readonly SynchronizationContext? _uiContext;

    public GameStateService(IHttpApiClient httpClient, ISignalRService signalRService, TelemetryService telemetryService, IClientIdProvider clientIdProvider, IClientSessionStorage sessionStorage, ILogger<GameStateService> logger)
    {
        _httpClient = httpClient;
        _signalRService = signalRService;
        _telemetryService = telemetryService;
        _clientIdProvider = clientIdProvider;
        _sessionStorage = sessionStorage;
        _logger = logger;
        _activitySource = new ActivitySource("StarConflictsRevolt.Blazor");
        
        // Subscribe to SignalR updates
        _signalRService.FullWorldReceived += OnWorldUpdated;
        _signalRService.UpdatesReceived += OnSignalRUpdates;
        _signalRService.ConnectionClosed += OnSignalRConnectionClosed;
        _signalRService.Reconnected += OnSignalRReconnected;

        // Capture the Blazor UI synchronization context (if created on the UI thread).
        _uiContext = SynchronizationContext.Current;
        
        _logger.LogInformation("GameStateService initialized");
    }

    public WorldDto? CurrentWorld => _currentWorld;
    public SessionDto? CurrentSession => _currentSession;
    public Guid? CurrentPlayerId => _currentPlayerId;
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
            await EnsureAuthContextAsync();
            // Check connection status before making request
            await CheckConnectionStatusAsync();
            
            _telemetryService.RecordHttpRequest();
            var stopwatch = Stopwatch.StartNew();
            
            var clientId = await _clientIdProvider.GetClientIdAsync(CancellationToken.None);
            _logger.LogDebug("Sending create session request to server (ClientId: {ClientId})", clientId?.Length > 0 ? clientId[..Math.Min(8, clientId.Length)] + "…" : "none");
            var sessionResponse = await _httpClient.CreateNewSessionAsync(sessionName, "SinglePlayer", clientId);
            
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
                _currentPlayerId = sessionResponse.PlayerId;
                _currentWorld = sessionResponse.World;
                if (_currentWorld == null)
                    _logger.LogWarning("Session created but server returned no world data for {SessionId}", sessionResponse.SessionId);

                _logger.LogDebug("Joining SignalR session {SessionId}", sessionResponse.SessionId);
                await _signalRService.JoinSessionAsync(sessionResponse.SessionId);

                await _sessionStorage.SetSessionIdAsync(sessionResponse.SessionId);
                var playerName = await _sessionStorage.GetPlayerNameAsync() ?? DefaultPlayerName;
                await _sessionStorage.SetPlayerNameAsync(playerName);

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

    public async Task<bool> TryRestoreSessionAsync()
    {
        var sessionId = await _sessionStorage.GetSessionIdAsync();
        if (!sessionId.HasValue)
            return false;
        _logger.LogInformation("Restoring session from storage: {SessionId}", sessionId);
        return await JoinSessionAsync(sessionId.Value);
    }

    public async Task<bool> JoinSessionAsync(Guid sessionId)
    {
        _logger.LogInformation("Joining session: {SessionId}", sessionId);
        var playerName = await _sessionStorage.GetPlayerNameAsync() ?? DefaultPlayerName;

        try
        {
            await EnsureAuthContextAsync();
            await CheckConnectionStatusAsync();

            var sessionResponse = await _httpClient.JoinSessionAsync(sessionId, playerName);
            if (sessionResponse != null)
            {
                _logger.LogInformation("Successfully joined session {SessionId} as {PlayerName}", sessionId, playerName);

                _currentSession = new SessionDto
                {
                    Id = sessionResponse.SessionId,
                    SessionName = "Joined Session",
                    SessionType = "Multiplayer",
                    Created = DateTime.UtcNow,
                    IsActive = true
                };
                _currentPlayerId = sessionResponse.PlayerId;
                _currentWorld = sessionResponse.World;
                if (_currentWorld == null)
                    _logger.LogWarning("Joined session but server returned no world data for {SessionId}", sessionId);

                _logger.LogDebug("Joining SignalR session {SessionId}", sessionId);
                await _signalRService.JoinSessionAsync(sessionId);

                await _sessionStorage.SetSessionIdAsync(sessionId);
                await _sessionStorage.SetPlayerNameAsync(playerName);

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
            await _sessionStorage.SetSessionIdAsync(null);
            await _signalRService.StopAsync();
            _currentSession = null;
            _currentWorld = null;
            _currentPlayerId = null;
            NotifyStateChanged();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error leaving session: {Message}", ex.Message);
        }
        return false;
    }

    public async Task<List<SessionDto>> GetAvailableSessionsAsync()
    {
        try
        {
            await EnsureAuthContextAsync();
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

    public async Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        try
        {
            await EnsureAuthContextAsync();
            var deleted = await _httpClient.DeleteSessionAsync(sessionId);
            if (deleted && _currentSession?.Id == sessionId)
            {
                await _sessionStorage.SetSessionIdAsync(null);
                _currentSession = null;
                _currentWorld = null;
                NotifyStateChanged();
            }
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> MoveFleetAsync(Guid fleetId, Guid fromPlanetId, Guid toPlanetId)
    {
        try
        {
            await EnsureAuthContextAsync();
            var worldId = _currentSession?.Id;
            if (!worldId.HasValue)
            {
                _logger.LogWarning("MoveFleetAsync called with no active session; server will return 404 without worldId.");
                return false;
            }
            var clientId = await _clientIdProvider.GetClientIdAsync(CancellationToken.None);
            var playerId = Guid.TryParse(clientId, out var g) ? g : Guid.Empty;
            var result = await _httpClient.MoveFleetAsync(fleetId, fromPlanetId, toPlanetId, worldId, playerId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving fleet: {Message}", ex.Message);
            return false;
        }
    }

    public async Task<bool> BuildStructureAsync(Guid planetId, string structureType)
    {
        try
        {
            if (_currentSession?.Id == null || !_currentPlayerId.HasValue)
            {
                _logger.LogWarning("BuildStructure requires an active session and player id");
                return false;
            }
            await EnsureAuthContextAsync();
            var result = await _httpClient.BuildStructureAsync(planetId, structureType, _currentSession.Id, _currentPlayerId.Value, CancellationToken.None);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building structure: {Message}", ex.Message);
            return false;
        }
    }

    public async Task<bool> AttackAsync(Guid attackerFleetId, Guid targetFleetId)
    {
        return await AttackAsync(attackerFleetId, targetFleetId, Guid.Empty);
    }

    public async Task<bool> AttackAsync(Guid attackerFleetId, Guid defenderFleetId, Guid locationPlanetId)
    {
        try
        {
            await EnsureAuthContextAsync();
            var result = await _httpClient.AttackAsync(attackerFleetId, defenderFleetId, locationPlanetId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error attacking");
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

    private async Task EnsureAuthContextAsync()
    {
        var clientId = await _clientIdProvider.GetClientIdAsync(CancellationToken.None);
        AuthClientIdContext.Current = string.IsNullOrWhiteSpace(clientId) ? null : clientId;
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
            await EnsureAuthContextAsync();
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

        // If we are on a SignalR/background thread, marshal back to the Blazor UI context
        // before notifying subscribers (which often call StateHasChanged()).
        if (_uiContext != null && SynchronizationContext.Current != _uiContext)
        {
            _uiContext.Post(static state =>
            {
                var svc = (GameStateService)state!;
                svc.StateChanged?.Invoke();
            }, this);

            return;
        }

        StateChanged?.Invoke();
    }
}
