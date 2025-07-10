using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Http.Authentication;
using StarConflictsRevolt.Clients.Raylib.Renderers;

namespace StarConflictsRevolt.Clients.Raylib.Services;

public class ClientInitializer : IClientInitializer
{
    private readonly ILogger<ClientInitializer> _logger;
    private readonly IConfiguration _configuration;
    private readonly IClientIdentityService _identityService;
    private readonly RenderContext _renderContext;
    private readonly ITokenProvider _tokenProvider;

    public ClientInitializer(
        ILogger<ClientInitializer> logger,
        IConfiguration configuration,
        IClientIdentityService identityService,
        RenderContext renderContext,
        ITokenProvider tokenProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _identityService = identityService;
        _renderContext = renderContext;
        _tokenProvider = tokenProvider;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting StarConflictsRevolt Raylib Client initialization");

        // Validate configuration
        ValidateConfiguration();

        // Setup client identity
        await SetupClientIdentityAsync();

        // Test authentication
        await TestAuthenticationAsync();

        _logger.LogInformation("Client initialization completed");
    }

    private void ValidateConfiguration()
    {
        _logger.LogInformation("Configuration sections: {Sections}", 
            string.Join(", ", _configuration.GetChildren().Select(c => c.Key)));
        _logger.LogInformation("TokenProviderOptions section exists: {Exists}", 
            _configuration.GetSection("TokenProviderOptions").Exists());
        _logger.LogInformation("TokenProviderOptions ClientId: {ClientId}", 
            _configuration["TokenProviderOptions:ClientId"]);

        var apiBaseUrl = _configuration["GameClientConfiguration:ApiBaseUrl"];
        var hubUrl = _configuration["GameClientConfiguration:GameServerHubUrl"];
        var tokenEndpoint = _configuration["TokenProviderOptions:TokenEndpoint"];

        if (apiBaseUrl == "SET_BY_ASPIRE_OR_ENVIRONMENT" || 
            hubUrl == "SET_BY_ASPIRE_OR_ENVIRONMENT" || 
            tokenEndpoint == "SET_BY_ASPIRE_OR_ENVIRONMENT")
        {
            _logger.LogWarning("One or more critical configuration values (ApiBaseUrl, GameServerHubUrl, TokenEndpoint) are not set by Aspire or environment. The client will not work correctly.");
        }

        // Debug assertions
        System.Diagnostics.Debug.Assert(apiBaseUrl != "SET_BY_ASPIRE_OR_ENVIRONMENT", "Aspire did not override ApiBaseUrl");
        System.Diagnostics.Debug.Assert(hubUrl != "SET_BY_ASPIRE_OR_ENVIRONMENT", "Aspire did not override GameServerHubUrl");
        System.Diagnostics.Debug.Assert(tokenEndpoint != "SET_BY_ASPIRE_OR_ENVIRONMENT", "Aspire did not override TokenEndpoint");
    }

    private async Task SetupClientIdentityAsync()
    {
        _logger.LogInformation("Setting up client identity");

        // Get or create client ID
        var clientId = _identityService.GetOrCreateClientId();

        // Get user profile
        var userProfile = _identityService.GetUserProfile();

        // Setup render context
        _renderContext.GameState.PlayerName = userProfile.DisplayName;
        _renderContext.GameState.PlayerId = userProfile.UserId;
        _renderContext.ClientId = clientId;

        _logger.LogInformation("Client ID set: {ClientId}", _renderContext.ClientId);
    }

    private async Task TestAuthenticationAsync()
    {
        _logger.LogInformation("Testing token acquisition");
        try
        {
            var token = await _tokenProvider.GetTokenAsync();
            _renderContext.AccessToken = token;
            _logger.LogInformation("Successfully obtained access token: {TokenPrefix}...", 
                token.Substring(0, Math.Min(10, token.Length)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to obtain access token during startup - will retry when needed");
            _renderContext.AccessToken = null;
        }
    }
} 