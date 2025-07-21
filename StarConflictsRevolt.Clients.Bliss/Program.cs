using Bliss.CSharp.Windowing;
using StarConflictsRevolt.Clients.Bliss.Infrastructure.Configuration;
using StarConflictsRevolt.Clients.Bliss.Core.UI;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;
using Veldrid;
using System.Text.Json;
using StarConflictsRevolt.Clients.Models;

var builder = Host.CreateApplicationBuilder();

var windowConfig = WindowConfiguration.Create16x9();

builder.Services.Configure<WindowConfiguration>(config => windowConfig.Set(config));

// Add UI services
builder.Services.AddUIServices();
builder.Services.ConfigureScreenManager();

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

// Register the simplified render loop
builder.Services.AddSingleton<SimpleRenderLoop>();

var app = builder.Build();

// Start the DI container and any background services
await app.StartAsync();

try
{
    // Initialize the screen manager
    var screenManagerInitializer = app.Services.GetRequiredService<IScreenManagerInitializer>();
    screenManagerInitializer.Initialize();
    
    // Get the simplified render loop from DI and run it on the main thread
    var renderLoop = app.Services.GetRequiredService<SimpleRenderLoop>();
    renderLoop.Run(); // This will block the main thread until the window closes
}
finally
{
    // Ensure proper cleanup when the render loop ends
    await app.StopAsync();
}

var configArg = args.FirstOrDefault(a => a.StartsWith("--config"));
if (configArg != null)
{
    var configPath = configArg.Split(' ', 2).Length > 1 ? configArg.Split(' ', 2)[1] : args.SkipWhile(a => a != "--config").Skip(1).FirstOrDefault();
    if (!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath))
    {
        var configJson = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<GameClientConfiguration>(configJson);
        Console.WriteLine($"Loaded config: GameServerUrl={config?.GameServerUrl}, GameServerHubUrl={config?.GameServerHubUrl}, ApiBaseUrl={config?.ApiBaseUrl}");
        // TODO: Use config for further initialization
    }
    else
    {
        Console.WriteLine($"Config file not found: {configPath}");
    }
}