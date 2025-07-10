using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Raylib.Renderers;
using StarConflictsRevolt.Clients.Raylib.Services;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestClientInitializer : IClientInitializer
{
    private readonly ILogger<TestClientInitializer> _logger;
    private readonly RenderContext _renderContext;
    private readonly IClientIdentityService _identityService;

    public TestClientInitializer(
        ILogger<TestClientInitializer> logger,
        RenderContext renderContext,
        IClientIdentityService identityService)
    {
        _logger = logger;
        _renderContext = renderContext;
        _identityService = identityService;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing test client");
        
        var clientId = _identityService.GetOrCreateClientId();
        var userProfile = _identityService.GetUserProfile();

        _renderContext.GameState.PlayerName = userProfile.DisplayName;
        _renderContext.GameState.PlayerId = userProfile.UserId;
        _renderContext.ClientId = clientId;

        _logger.LogInformation("Test client initialized");
    }
}