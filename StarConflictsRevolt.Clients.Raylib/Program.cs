using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Shared;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

// Add custom file logging provider
builder.Logging.AddProvider(new FileLoggerProvider());
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting StarConflictsRevolt Raylib Client");

// Add HTTP client factory for Clients.Shared integration
builder.Services.AddHttpClient();
HttpApiClient.AddHttpApiClientWithAuth(builder.Services, "GameApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5153");
    logger.LogInformation("Configured HttpApiClient with base address: {BaseAddress}", client.BaseAddress);
});

builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>();
builder.Services.AddSingleton<IGameRenderer, RaylibRenderer>();
builder.Services.AddSingleton<RenderContext>();
builder.Services.AddSingleton<GameCommandService>();
builder.Services.AddSingleton<GameState>();

// Register all view renderers
builder.Services.AddSingleton<IView, MenuView>(sp => 
    new MenuView(sp.GetRequiredService<RenderContext>(), sp.GetRequiredService<GameCommandService>()));
builder.Services.AddSingleton<IView, GalaxyView>(sp => 
    new GalaxyView(sp.GetRequiredService<RenderContext>(), sp.GetRequiredService<GameCommandService>()));
builder.Services.AddSingleton<IView, TacticalBattleView>(sp => 
    new TacticalBattleView(sp.GetRequiredService<RenderContext>(), sp.GetRequiredService<GameCommandService>()));
builder.Services.AddSingleton<IView, FleetFinderView>(sp => 
    new FleetFinderView(sp.GetRequiredService<RenderContext>(), sp.GetRequiredService<GameCommandService>()));
builder.Services.AddSingleton<IView, GameOptionsView>(sp => 
    new GameOptionsView(sp.GetRequiredService<RenderContext>()));
builder.Services.AddSingleton<IView, PlanetaryFinderView>(sp => 
    new PlanetaryFinderView(sp.GetRequiredService<RenderContext>(), sp.GetRequiredService<GameCommandService>()));

// Bind GameClientConfiguration from configuration
builder.Services.Configure<GameClientConfiguration>(
    builder.Configuration.GetSection("GameClientConfiguration"));

builder.Services.AddSingleton<SignalRService>();
builder.Services.AddHostedService<ClientServiceHost>();

logger.LogInformation("Service registration completed");

var host = builder.Build();

// --- Client identity and authentication setup ---
logger.LogInformation("Starting client identity and authentication setup");

var renderContext = host.Services.GetRequiredService<RenderContext>();

// Get user profile information
logger.LogInformation("Retrieving Windows user profile");
var userProfile = UserProfile.GetUserProfile();
logger.LogInformation("User profile retrieved: UserId={UserId}, DisplayName={DisplayName}, UserName={UserName}", 
    userProfile.UserId, userProfile.DisplayName, userProfile.UserName);

renderContext.GameState.PlayerName = userProfile.DisplayName;
renderContext.GameState.PlayerId = userProfile.UserId;

// Get or create client ID from app data
logger.LogInformation("Setting up client ID from app data");
var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StarConflictsRevolt");
Directory.CreateDirectory(appDataPath);
logger.LogInformation("App data directory: {AppDataPath}", appDataPath);

var clientIdFile = Path.Combine(appDataPath, "client_id.txt");

if (!File.Exists(clientIdFile))
{
    var clientId = $"client-{Guid.NewGuid().ToString().Substring(0, 8)}";
    File.WriteAllText(clientIdFile, clientId);
    logger.LogInformation("Created new client ID: {ClientId}", clientId);
}
else
{
    var existingClientId = File.ReadAllText(clientIdFile);
    logger.LogInformation("Using existing client ID: {ClientId}", existingClientId);
}

renderContext.ClientId = File.ReadAllText(clientIdFile);
logger.LogInformation("Client ID set: {ClientId}", renderContext.ClientId);

// Obtain JWT access token from API using HttpApiClient
logger.LogInformation("Attempting to obtain JWT access token from API");
var httpApiClient = host.Services.GetRequiredService<HttpApiClient>();
var tokenRequest = new { client_id = renderContext.ClientId, secret = "changeme" };
logger.LogInformation("Sending token request with client_id: {ClientId}", renderContext.ClientId);

var tokenResponse = await httpApiClient.PostAsync("/token", tokenRequest);

if (tokenResponse.IsSuccessStatusCode)
{
    var json = await tokenResponse.Content.ReadAsStringAsync();
    var doc = JsonDocument.Parse(json);
    renderContext.AccessToken = doc.RootElement.GetProperty("access_token").GetString();
    logger.LogInformation("Successfully obtained access token: {TokenPrefix}...", 
        renderContext.AccessToken?.Substring(0, Math.Min(10, renderContext.AccessToken.Length)));
}
else
{
    var errorContent = await tokenResponse.Content.ReadAsStringAsync();
    logger.LogWarning("Failed to obtain access token. Status: {StatusCode}, Error: {Error}", 
        tokenResponse.StatusCode, errorContent);
    renderContext.AccessToken = null;
}

logger.LogInformation("Client setup completed. Starting host...");

host.Run();