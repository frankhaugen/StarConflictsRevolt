namespace StarConflictsRevolt.Clients.Bliss.Core;

/// <summary>
/// Drives the main game loop with update and draw cycles.
/// </summary>
public interface IGameLoop
{
    /// <summary>
    /// Runs the game loop asynchronously until cancellation is requested.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the game loop</param>
    /// <returns>Task that completes when the game loop ends</returns>
    Task RunAsync(CancellationToken cancellationToken);
} 