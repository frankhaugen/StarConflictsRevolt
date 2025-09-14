using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Shared.Communication;

namespace StarConflictsRevolt.Clients.Blazor.Services;

public class GameStateService : IGameStateService
{
    private readonly IHttpApiClient _httpClient;
    private readonly ISignalRService _signalRService;
    private WorldDto? _currentWorld;
    private SessionDto? _currentSession;

    public GameStateService(IHttpApiClient httpClient, ISignalRService signalRService)
    {
        _httpClient = httpClient;
        _signalRService = signalRService;
        
        // Subscribe to SignalR updates
        _signalRService.FullWorldReceived += OnWorldUpdated;
    }

    public WorldDto? CurrentWorld => _currentWorld;
    public SessionDto? CurrentSession => _currentSession;
    public bool IsConnected => true; // TODO: Implement proper connection status

    public event Action? StateChanged;

    public async Task<bool> CreateSessionAsync(string sessionName)
    {
        try
        {
            var sessionResponse = await _httpClient.CreateNewSessionAsync(sessionName, "SinglePlayer");
            if (sessionResponse != null)
            {
                _currentSession = new SessionDto
                {
                    Id = sessionResponse.SessionId,
                    SessionName = sessionName,
                    SessionType = "SinglePlayer",
                    Created = DateTime.UtcNow,
                    IsActive = true
                };
                _currentWorld = sessionResponse.World;
                await _signalRService.JoinSessionAsync(sessionResponse.SessionId);
                NotifyStateChanged();
                return true;
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error creating session: {ex.Message}");
        }
        return false;
    }

    public async Task<bool> JoinSessionAsync(Guid sessionId)
    {
        try
        {
            var sessionResponse = await _httpClient.JoinSessionAsync(sessionId, "Player");
            if (sessionResponse != null)
            {
                _currentSession = new SessionDto
                {
                    Id = sessionResponse.SessionId,
                    SessionName = "Joined Session",
                    SessionType = "Multiplayer",
                    Created = DateTime.UtcNow,
                    IsActive = true
                };
                _currentWorld = sessionResponse.World;
                await _signalRService.JoinSessionAsync(sessionId);
                NotifyStateChanged();
                return true;
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error joining session: {ex.Message}");
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


    private void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
