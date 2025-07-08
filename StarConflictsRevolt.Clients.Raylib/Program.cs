using Microsoft.Extensions.Options;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Shared;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>();
builder.Services.AddSingleton<IGameRenderer, RaylibRenderer>();
builder.Services.AddSingleton<RenderContext>();
builder.Services.AddSingleton<IView, MenuView>();
builder.Services.AddSingleton<IView, GalaxyView>();

// Bind GameClientConfiguration from configuration
builder.Services.Configure<GameClientConfiguration>(
    builder.Configuration.GetSection("GameClientConfiguration"));

builder.Services.AddSingleton<SignalRService>();
builder.Services.AddHostedService<ClientServiceHost>();

var host = builder.Build();
host.Run();