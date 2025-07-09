using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Aspire.ServiceDefaults;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

// Add ServiceDefaults for service discovery and observability
builder.AddServiceDefaults();

// Add custom file logging provider
builder.Logging.AddProvider(new FileLoggerProvider());
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting StarConflictsRevolt Raylib Client");

// Get or create client ID from app data first (needed for token provider configuration)
var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StarConflictsRevolt");
Directory.CreateDirectory(appDataPath);
logger.LogInformation("App data directory: {AppDataPath}", appDataPath);

var clientIdFile = Path.Combine(appDataPath, "client_id.txt");
string clientId;

if (!File.Exists(clientIdFile))
{
    clientId = $"client-{Guid.NewGuid().ToString().Substring(0, 8)}";
    File.WriteAllText(clientIdFile, clientId);
    logger.LogInformation("Created new client ID: {ClientId}", clientId);
}
else
{
    clientId = File.ReadAllText(clientIdFile);
    logger.LogInformation("Using existing client ID: {ClientId}", clientId);
}

// Add HTTP client factory for Clients.Shared integration
builder.Services.AddHttpClient();

// Configure TokenProvider options BEFORE registering the service
builder.Services.Configure<TokenProviderOptions>(options =>
{
    // Use service discovery for the WebApi service
    options.TokenEndpoint = "http://webapi/token";
    options.ClientId = clientId;
    options.Secret = "changeme";
});

// Register TokenProvider AFTER configuration
builder.Services.AddSingleton<ITokenProvider, CachingTokenProvider>();

// Configure HTTP client with service discovery using HttpApiClient
HttpApiClient.AddHttpApiClientWithAuth(builder.Services, "GameApi", client =>
{
    // Use service discovery to find the WebApi service
    client.BaseAddress = new Uri("http://webapi");
    logger.LogInformation("Configured HttpApiClient with service discovery for WebApi");
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
renderContext.ClientId = clientId;

logger.LogInformation("Client ID set: {ClientId}", renderContext.ClientId);

// Test token acquisition
logger.LogInformation("Testing token acquisition");
try
{
    var tokenProvider = host.Services.GetRequiredService<ITokenProvider>();
    var token = await tokenProvider.GetTokenAsync();
    renderContext.AccessToken = token;
    logger.LogInformation("Successfully obtained access token: {TokenPrefix}...", 
        token.Substring(0, Math.Min(10, token.Length)));
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Failed to obtain access token during startup");
    renderContext.AccessToken = null;
}

logger.LogInformation("Client setup completed. Starting host...");

host.Run();