using StarConflictsRevolt.Server.GameEngine;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<GameUpdateService>();

builder.Services.AddSignalR(config =>
{
    config.EnableDetailedErrors = true;
    config.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
});

var app = builder.Build();

app.MapHub<WorldHub>("/gamehub");

app.Run();
