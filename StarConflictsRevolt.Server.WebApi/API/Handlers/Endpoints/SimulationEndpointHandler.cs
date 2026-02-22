using StarConflictsRevolt.Server.Simulation.Engine;

namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>
/// Endpoints to read and change simulation speed (ticker) in real time.
/// </summary>
public static class SimulationEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/game/simulation", (ISimulationManager simulation) => Results.Ok(new SimulationStateDto
        {
            TicksPerSecond = simulation.TicksPerSecond,
            MinTicksPerSecond = simulation.MinTicksPerSecond,
            MaxTicksPerSecond = simulation.MaxTicksPerSecond,
            IsPaused = simulation.IsPaused
        })).WithName("GetSimulationState").AllowAnonymous();

        app.MapPatch("/game/simulation", async (HttpContext context, ISimulationManager simulation) =>
        {
            var body = await context.Request.ReadFromJsonAsync<SimulationPatchDto>(context.RequestAborted);
            if (body == null)
                return Results.BadRequest("Body required: { ticksPerSecond?: number, isPaused?: boolean }");
            if (body.TicksPerSecond.HasValue)
                simulation.SetTicksPerSecond(body.TicksPerSecond.Value);
            if (body.IsPaused.HasValue)
                simulation.SetPaused(body.IsPaused.Value);
            return Results.Ok(new SimulationStateDto
            {
                TicksPerSecond = simulation.TicksPerSecond,
                MinTicksPerSecond = simulation.MinTicksPerSecond,
                MaxTicksPerSecond = simulation.MaxTicksPerSecond,
                IsPaused = simulation.IsPaused
            });
        }).WithName("PatchSimulation").AllowAnonymous();
    }

    public sealed class SimulationStateDto
    {
        public int TicksPerSecond { get; init; }
        public int MinTicksPerSecond { get; init; }
        public int MaxTicksPerSecond { get; init; }
        public bool IsPaused { get; init; }
    }

    public sealed class SimulationPatchDto
    {
        public int? TicksPerSecond { get; set; }
        public bool? IsPaused { get; set; }
    }
}
