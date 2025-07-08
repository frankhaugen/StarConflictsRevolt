using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib;
using StarConflictsRevolt.Clients.Shared;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>();
builder.Services.AddSingleton<IGameRenderer, RaylibRenderer>();

// Bind GameClientConfiguration from configuration
builder.Services.Configure<GameClientConfiguration>(
    builder.Configuration.GetSection("GameClientConfiguration"));

builder.Services.AddSingleton<RenderService>();
builder.Services.AddHostedService<RenderServiceHost>();

var host = builder.Build();
host.Run();