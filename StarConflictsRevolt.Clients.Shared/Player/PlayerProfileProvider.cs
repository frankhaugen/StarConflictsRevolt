using System.Text.Json;

namespace StarConflictsRevolt.Clients.Shared.Player;

public class PlayerProfileProvider : IPlayerProfileProvider
{
    private const string ProfileFileName = "player_profile.json";
    private PlayerProfile? _currentProfile;

    public PlayerProfile? GetPlayerProfile()
    {
        return _currentProfile;
    }

    public void SetPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name cannot be null or empty.", nameof(name));

        if (_currentProfile == null)
        {
            CreatePlayerProfile(name);
        }
        else
        {
            _currentProfile = _currentProfile with { Name = name, LastAccessed = DateTime.UtcNow };
            SavePlayerProfile();
        }
    }

    public bool HasPlayerProfile()
    {
        return _currentProfile != null;
    }

    public void CreatePlayerProfile(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name cannot be null or empty.", nameof(name));

        _currentProfile = new PlayerProfile
        {
            Name = name,
            CreatedAt = DateTime.UtcNow,
            LastAccessed = DateTime.UtcNow
        };

        SavePlayerProfile();
    }

    public void SavePlayerProfile()
    {
        if (_currentProfile == null)
            return;

        try
        {
            var json = JsonSerializer.Serialize(_currentProfile, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var profilePath = GetProfilePath();
            File.WriteAllText(profilePath, json);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - profile saving is not critical for app functionality
            Console.WriteLine($"Failed to save player profile: {ex.Message}");
        }
    }

    public void LoadPlayerProfile()
    {
        try
        {
            var profilePath = GetProfilePath();
            if (!File.Exists(profilePath))
                return;

            var json = File.ReadAllText(profilePath);
            _currentProfile = JsonSerializer.Deserialize<PlayerProfile>(json);

            if (_currentProfile != null)
            {
                _currentProfile = _currentProfile with { LastAccessed = DateTime.UtcNow };
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - profile loading is not critical for app functionality
            Console.WriteLine($"Failed to load player profile: {ex.Message}");
            _currentProfile = null;
        }
    }

    private static string GetProfilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "StarConflictsRevolt");

        // Ensure the directory exists
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        return Path.Combine(appFolder, ProfileFileName);
    }
} 