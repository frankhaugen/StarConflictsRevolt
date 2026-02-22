namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>
///     Handles button/action reporting so every button click is sent to the backend and executed (logged).
/// </summary>
public static class ActionEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        app.MapPost("/game/action", async context =>
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Game.Action");
                var body = await context.Request.ReadFromJsonAsync<ButtonActionRequest>(context.RequestAborted);
                if (body == null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid body");
                    return;
                }
                logger.LogInformation(
                    "Button action: Label={Label}, ButtonId={ButtonId}, Page={Page}, SessionId={SessionId}, Timestamp={Timestamp}",
                    body.Label ?? "(empty)",
                    body.ButtonId ?? "(none)",
                    body.Page ?? "(none)",
                    body.SessionId,
                    body.Timestamp);
                context.Response.StatusCode = 204;
            })
            .WithName("ReportButtonAction")
            .AllowAnonymous();
    }

    private sealed class ButtonActionRequest
    {
        public string? Label { get; set; }
        public string? ButtonId { get; set; }
        public string? Page { get; set; }
        public Guid? SessionId { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
