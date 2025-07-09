using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Clients.Raylib;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Shared;
using System.Text.Json;
using System.Security.Principal;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder(args);

// Add HTTP client factory for Clients.Shared integration
builder.Services.AddHttpClient();
HttpApiClient.AddHttpApiClientWithAuth(builder.Services, "GameApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5153");
});

builder.Services.AddSingleton<IClientWorldStore, ClientWorldStore>();
builder.Services.AddSingleton<IGameRenderer, RaylibRenderer>();
builder.Services.AddSingleton<RenderContext>();
builder.Services.AddSingleton<GameCommandService>();

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

var host = builder.Build();

// --- Client identity and authentication setup ---
var renderContext = host.Services.GetRequiredService<RenderContext>();

// Get user profile information
var userProfile = GetUserProfile();
renderContext.GameState.PlayerName = userProfile.DisplayName;
renderContext.GameState.PlayerId = userProfile.UserId;

// Get or create client ID from app data
var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StarConflictsRevolt");
Directory.CreateDirectory(appDataPath);
var clientIdFile = Path.Combine(appDataPath, "client_id.txt");

if (!File.Exists(clientIdFile))
{
    var clientId = $"client-{Guid.NewGuid().ToString().Substring(0, 8)}";
    File.WriteAllText(clientIdFile, clientId);
}
renderContext.ClientId = File.ReadAllText(clientIdFile);

// Obtain JWT access token from API using HttpApiClient
var httpApiClient = host.Services.GetRequiredService<HttpApiClient>();
var tokenRequest = new { client_id = renderContext.ClientId, secret = "changeme" };
var tokenResponse = await httpApiClient.PostAsync("/token", tokenRequest);

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

// Helper method to get Windows user profile
static UserProfile GetUserProfile()
{
    try
    {
        var identity = WindowsIdentity.GetCurrent();
        var name = identity?.Name ?? "Unknown User";
        
        // Try to get display name from Windows
        string displayName = name;
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var userPrincipal = System.DirectoryServices.AccountManagement.UserPrincipal.Current;
                if (userPrincipal != null)
                {
                    displayName = userPrincipal.DisplayName ?? userPrincipal.Name ?? name;
                }
            }
        }
        catch
        {
            // Fallback to username if display name lookup fails
            displayName = name;
        }
        
        return new UserProfile
        {
            UserId = identity?.User?.Value ?? Guid.NewGuid().ToString(),
            DisplayName = displayName,
            UserName = name
        };
    }
    catch
    {
        return new UserProfile
        {
            UserId = Guid.NewGuid().ToString(),
            DisplayName = "Unknown User",
            UserName = "Unknown"
        };
    }
}

public record UserProfile
{
    public string UserId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
}