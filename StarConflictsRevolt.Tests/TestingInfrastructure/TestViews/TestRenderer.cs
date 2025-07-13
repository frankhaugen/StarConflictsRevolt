using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.TestViews;

public class TestRenderer : IGameRenderer
{
    /// <inheritdoc />
    public async Task<bool> RenderAsync(WorldDto? world, CancellationToken cancellationToken) => true;
}