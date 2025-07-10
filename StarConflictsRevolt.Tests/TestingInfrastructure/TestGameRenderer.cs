using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestGameRenderer : IGameRenderer
{
    public bool WasRendered { get; private set; }
    public WorldDto? LastRenderedWorld { get; private set; }

    public Task<bool> RenderAsync(WorldDto? world, CancellationToken cancellationToken)
    {
        WasRendered = true;
        LastRenderedWorld = world;
        return Task.FromResult(true);
    }
}