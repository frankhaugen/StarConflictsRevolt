using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Server.WebApi;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Server.Core;

var builder = WebApplication.CreateBuilder(args);

// Register RavenDB DocumentStore
builder.Services.AddSingleton<IDocumentStore>(_ => new DocumentStore
{
    Urls = new[] { "http://localhost:8080" }, // TODO: Make configurable
    Database = "StarConflictsRevolt"
}.Initialize());

// Register RavenEventStore as IEventStore
builder.Services.AddSingleton<IEventStore, RavenEventStore>();

// Register CommandQueue as singleton for DI
builder.Services.AddSingleton(typeof(CommandQueue<IGameEvent>));

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/game/state", async context =>
    {
        var worldService = context.RequestServices.GetRequiredService<WorldService>();
        var world = await worldService.GetWorldAsync(context.RequestAborted);
        if (world == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("World not found");
            return;
        }

        await context.Response.WriteAsJsonAsync(world, context.RequestAborted);
    })
    .WithName("GetGameState");

app.MapPost("/game/session", async context =>
    {
        var sessionService = context.RequestServices.GetRequiredService<SessionService>();
        var sessionName = await context.Request.ReadFromJsonAsync<string>(context.RequestAborted);
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Session name is required");
            return;
        }

        var sessionId = await sessionService.CreateSessionAsync(sessionName, context.RequestAborted);
        context.Response.StatusCode = 201;
        await context.Response.WriteAsJsonAsync(new { SessionId = sessionId }, context.RequestAborted);
    })
    .WithName("CreateGameSession");

app.MapPost("/game/move-fleet", async context =>
{
    var commandQueue = context.RequestServices.GetRequiredService<CommandQueue<IGameEvent>>();
    var dto = await context.Request.ReadFromJsonAsync<MoveFleetEvent>(context.RequestAborted);
    if (dto == null)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid MoveFleetEvent");
        return;
    }
    // TODO: Get session/world ID from query/body/auth
    var worldId = context.Request.Query.ContainsKey("worldId") ? Guid.Parse(context.Request.Query["worldId"]) : Guid.Empty;
    commandQueue.Enqueue(worldId, dto);
    context.Response.StatusCode = 202;
});

app.MapPost("/game/build-structure", async context =>
{
    var commandQueue = context.RequestServices.GetRequiredService<CommandQueue<IGameEvent>>();
    var dto = await context.Request.ReadFromJsonAsync<BuildStructureEvent>(context.RequestAborted);
    if (dto == null)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid BuildStructureEvent");
        return;
    }
    var worldId = context.Request.Query.ContainsKey("worldId") ? Guid.Parse(context.Request.Query["worldId"]) : Guid.Empty;
    commandQueue.Enqueue(worldId, dto);
    context.Response.StatusCode = 202;
});

app.MapPost("/game/attack", async context =>
{
    var commandQueue = context.RequestServices.GetRequiredService<CommandQueue<IGameEvent>>();
    var dto = await context.Request.ReadFromJsonAsync<AttackEvent>(context.RequestAborted);
    if (dto == null)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid AttackEvent");
        return;
    }
    var worldId = context.Request.Query.ContainsKey("worldId") ? Guid.Parse(context.Request.Query["worldId"]) : Guid.Empty;
    commandQueue.Enqueue(worldId, dto);
    context.Response.StatusCode = 202;
});

app.MapPost("/game/diplomacy", async context =>
{
    var commandQueue = context.RequestServices.GetRequiredService<CommandQueue<IGameEvent>>();
    var dto = await context.Request.ReadFromJsonAsync<DiplomacyEvent>(context.RequestAborted);
    if (dto == null)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid DiplomacyEvent");
        return;
    }
    var worldId = context.Request.Query.ContainsKey("worldId") ? Guid.Parse(context.Request.Query["worldId"]) : Guid.Empty;
    commandQueue.Enqueue(worldId, dto);
    context.Response.StatusCode = 202;
});

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
});

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
});

app.Run();