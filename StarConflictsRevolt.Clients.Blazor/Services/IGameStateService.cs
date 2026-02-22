using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Blazor.Services;

public interface IGameStateService
{
    WorldDto? CurrentWorld { get; }
    SessionDto? CurrentSession { get; }
    Guid? CurrentPlayerId { get; }
    bool IsConnected { get; }
    
    event Action? StateChanged;
    
    Task<bool> CreateSessionAsync(string sessionName);
    Task<bool> JoinSessionAsync(Guid sessionId);
    Task<bool> LeaveSessionAsync();
    Task<List<SessionDto>> GetAvailableSessionsAsync();
    Task<bool> DeleteSessionAsync(Guid sessionId);
    Task<bool> MoveFleetAsync(Guid fleetId, Guid fromPlanetId, Guid toPlanetId);
    Task<bool> BuildStructureAsync(Guid planetId, string structureType);
    Task<bool> AttackAsync(Guid attackerFleetId, Guid targetFleetId);
    Task<bool> AttackAsync(Guid attackerFleetId, Guid defenderFleetId, Guid locationPlanetId);
}
