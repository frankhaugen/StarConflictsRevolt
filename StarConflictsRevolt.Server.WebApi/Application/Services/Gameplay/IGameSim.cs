using StarConflictsRevolt.Server.WebApi.Core.Domain.Commands;
using StarConflictsRevolt.Server.EventStorage.Abstractions;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;
using WorldState = StarConflictsRevolt.Server.WebApi.Core.Domain.World.World;

namespace StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;

/// <summary>
/// Validates commands against current world and produces events (facts).
/// </summary>
public interface IGameSim
{
    IReadOnlyList<IGameEvent> Execute(long tick, WorldState world, IGameCommand command);
}
