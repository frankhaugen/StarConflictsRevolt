namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;

/// <summary>
/// Player intent (request). Commands are submitted by clients and processed by the sim;
/// the sim produces events (facts), not commands.
/// </summary>
public interface IGameCommand
{
    Guid PlayerId { get; }
    long ClientTick { get; }
}
