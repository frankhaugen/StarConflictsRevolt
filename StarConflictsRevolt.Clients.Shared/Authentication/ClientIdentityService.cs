using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Clients.Shared.User;

namespace StarConflictsRevolt.Clients.Shared.Authentication;

/// <summary>
/// Generic client identity service implementation that can be used by both Raylib and Bliss clients.
/// This version removes platform-specific dependencies.
/// </summary>
public class ClientIdentityService : IClientIdentityService
{
    private readonly ILogger<ClientIdentityService> _logger;
    private readonly IUserProfileProvider _userProfileProvider;

    public ClientIdentityService(ILogger<ClientIdentityService> logger, IUserProfileProvider userProfileProvider)
    {
        _logger = logger;
        _userProfileProvider = userProfileProvider;
    }

    public string GetOrCreateClientId()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StarConflictsRevolt");
        Directory.CreateDirectory(appDataPath);
        _logger.LogInformation("App data directory: {AppDataPath}", appDataPath);

        var clientIdFile = Path.Combine(appDataPath, "client_id.txt");
        string clientId;

        if (!File.Exists(clientIdFile))
        {
            clientId = $"client-{Guid.NewGuid().ToString().Substring(0, 8)}";
            File.WriteAllText(clientIdFile, clientId);
            _logger.LogInformation("Created new client ID: {ClientId}", clientId);
        }
        else
        {
            clientId = File.ReadAllText(clientIdFile);
            _logger.LogInformation("Using existing client ID: {ClientId}", clientId);
        }

        return clientId;
    }

    public IUserProfile GetUserProfile()
    {
        _logger.LogInformation("Retrieving user profile");
        var userProfile = _userProfileProvider.GetUserProfile();
        _logger.LogInformation("User profile retrieved: UserId={UserId}, DisplayName={DisplayName}, UserName={UserName}",
            userProfile.UserId, userProfile.DisplayName, userProfile.UserName);
        return userProfile;
    }
}