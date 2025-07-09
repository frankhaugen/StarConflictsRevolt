using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Shared;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>();
builder.Services.AddSingleton<IGameRenderer, RaylibRenderer>();
builder.Services.AddSingleton<RenderContext>();
builder.Services.AddSingleton<GameCommandService>();

// Register all view renderers
builder.Services.AddSingleton<IView, MenuView>();
builder.Services.AddSingleton<IView, GalaxyView>();
builder.Services.AddSingleton<IView, TacticalBattleView>();
builder.Services.AddSingleton<IView, FleetFinderView>();
builder.Services.AddSingleton<IView, GameOptionsView>();
builder.Services.AddSingleton<IView, PlanetaryFinderView>();

// Bind GameClientConfiguration from configuration
builder.Services.Configure<GameClientConfiguration>(
    builder.Configuration.GetSection("GameClientConfiguration"));

builder.Services.AddSingleton<SignalRService>();
builder.Services.AddHostedService<ClientServiceHost>();

var host = builder.Build();

// --- Client identity and authentication setup ---
var renderContext = host.Services.GetRequiredService<RenderContext>();
const string clientIdFile = "client_id.txt";
if (!File.Exists(clientIdFile))
{
    // Generate a simple passphrase (stub, replace with Frank.Security if available)
    var clientId = $"client-{Guid.NewGuid().ToString().Substring(0, 8)}";
    File.WriteAllText(clientIdFile, clientId);
}
renderContext.ClientId = File.ReadAllText(clientIdFile);

// Obtain JWT access token from API
var httpClient = new HttpClient();
var tokenResponse = await httpClient.PostAsync("http://localhost:5000/token", new StringContent($"{{\"client_id\":\"{renderContext.ClientId}\",\"secret\":\"changeme\"}}", System.Text.Encoding.UTF8, "application/json"));
if (tokenResponse.IsSuccessStatusCode)
{
    var json = await tokenResponse.Content.ReadAsStringAsync();
    var doc = JsonDocument.Parse(json);
    renderContext.AccessToken = doc.RootElement.GetProperty("access_token").GetString();
}
else
{
    renderContext.AccessToken = null;
}

host.Run();