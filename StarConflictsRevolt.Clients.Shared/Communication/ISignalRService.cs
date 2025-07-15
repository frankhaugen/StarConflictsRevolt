using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Shared.Communication;

/// <summary>
/// Generic interface for SignalR service that can be used by both Raylib and Bliss clients.
/// This removes the dependency on specific world store implementations.
/// </summary>
public interface ISignalRService : IAsyncDisposable
{
    /// <summary>
    /// Starts the SignalR connection
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Joins a game session
    /// </summary>
    /// <param name="sessionId">Session ID to join</param>
    Task JoinSessionAsync(Guid sessionId);

    /// <summary>
    /// Stops the SignalR connection
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Event that is raised when a full world update is received
    /// </summary>
    event Action<WorldDto>? FullWorldReceived;

    /// <summary>
    /// Event that is raised when delta updates are received
    /// </summary>
    event Action<List<GameObjectUpdate>>? UpdatesReceived;

    /// <summary>
    /// Event that is raised when the connection is closed
    /// </summary>
    event Action<Exception?>? ConnectionClosed;

    /// <summary>
    /// Event that is raised when the connection is reconnecting
    /// </summary>
    event Action<Exception?>? Reconnecting;

    /// <summary>
    /// Event that is raised when the connection is reconnected
    /// </summary>
    event Action<string>? Reconnected;
} 