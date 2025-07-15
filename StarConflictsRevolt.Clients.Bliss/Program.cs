using Bliss.CSharp.Windowing;
using StarConflictsRevolt.Clients.Bliss;
using StarConflictsRevolt.Clients.Bliss.Infrastructure.Configuration;
using Veldrid;

var builder = Host.CreateApplicationBuilder();

var windowConfig = WindowConfiguration.Create16x9();

builder.Services.Configure<WindowConfiguration>(config => windowConfig.Set(config));

var window = Window.CreateWindow(
    windowConfig.Type,
    windowConfig.Width,
    windowConfig.Height,
    windowConfig.Title,
    windowConfig.State,
    windowConfig.GraphicsDeviceOptions,
    windowConfig.GraphicsBackend,
    out var device
);

builder.Services.AddSingleton<IWindow>(window);
builder.Services.AddSingleton<GraphicsDevice>(device);

builder.Services.AddHostedService<RenderLoopService>();

var app = builder.Build();

await app.RunAsync();