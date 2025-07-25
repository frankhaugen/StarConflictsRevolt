using StarConflictsRevolt.Server.WebApi.Core.Domain.Events;

namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>
///     Handles event store and snapshot endpoints
/// </summary>
public static class EventEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapGet("/game/{worldId}/events", async context =>
        {
            var eventStore = context.RequestServices.GetRequiredService<IEventStore>() as RavenEventStore;
            if (eventStore == null)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Event store not available");
                return;
            }

            var worldIdStr = context.Request.RouteValues["worldId"]?.ToString();
            if (!Guid.TryParse(worldIdStr, out var worldId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid worldId");
                return;
            }

            var events = eventStore.GetEventsForWorld(worldId);
            await context.Response.WriteAsJsonAsync(events);
        }).RequireAuthorization();

        app.MapPost("/game/{worldId}/snapshot", async context =>
        {
            var eventStore = context.RequestServices.GetRequiredService<IEventStore>() as RavenEventStore;
            if (eventStore == null)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Event store not available");
                return;
            }

            var worldIdStr = context.Request.RouteValues["worldId"]?.ToString();
            if (!Guid.TryParse(worldIdStr, out var worldId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid worldId");
                return;
            }

            var worldState = await context.Request.ReadFromJsonAsync<object>(context.RequestAborted);
            eventStore.SnapshotWorld(worldId, worldState!);
            context.Response.StatusCode = 201;
        }).RequireAuthorization();
    }
}