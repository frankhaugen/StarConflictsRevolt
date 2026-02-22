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
    /// <summary>Restores and joins the session stored in sessionStorage using stored player name; returns false if no session stored or join fails.</summary>
    Task<bool> TryRestoreSessionAsync();
    Task<bool> JoinSessionAsync(Guid sessionId);
    Task<bool> LeaveSessionAsync();
    Task<List<SessionDto>> GetAvailableSessionsAsync();
    Task<bool> DeleteSessionAsync(Guid sessionId);
    Task<bool> MoveFleetAsync(Guid fleetId, Guid fromPlanetId, Guid toPlanetId);
    Task<bool> BuildStructureAsync(Guid planetId, string structureType);
    Task<bool> AttackAsync(Guid attackerFleetId, Guid targetFleetId);
    Task<bool> AttackAsync(Guid attackerFleetId, Guid defenderFleetId, Guid locationPlanetId);
}
