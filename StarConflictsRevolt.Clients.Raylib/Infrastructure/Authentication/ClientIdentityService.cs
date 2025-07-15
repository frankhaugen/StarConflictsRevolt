namespace StarConflictsRevolt.Clients.Raylib.Infrastructure.Authentication;

public class ClientIdentityService : IClientIdentityService
{
    private readonly ILogger<ClientIdentityService> _logger;
    
    public ClientIdentityService(ILogger<ClientIdentityService> logger)
    {
        _logger = logger;
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
}