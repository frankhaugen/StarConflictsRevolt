using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Blazor.Services;

public interface IGameStateService
{
    WorldDto? CurrentWorld { get; }
    SessionDto? CurrentSession { get; }
    bool IsConnected { get; }
    
    event Action? StateChanged;
    
    Task<bool> CreateSessionAsync(string sessionName);
    Task<bool> JoinSessionAsync(Guid sessionId);
    Task<bool> LeaveSessionAsync();
    Task<List<SessionDto>> GetAvailableSessionsAsync();
    Task<bool> MoveFleetAsync(Guid fleetId, Guid fromPlanetId, Guid toPlanetId);
    Task<bool> BuildStructureAsync(Guid planetId, string structureType);
    Task<bool> AttackAsync(Guid attackerFleetId, Guid targetFleetId);
}
