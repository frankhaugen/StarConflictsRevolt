using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Server.WebApi;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();