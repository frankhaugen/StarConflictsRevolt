using System.Text.Json;
using StarConflictsRevolt.Clients.Bliss.Core.UI.Interfaces;

namespace StarConflictsRevolt.Clients.Bliss.Core.UI;

/// <summary>
/// Provides access to user profile information with file-based persistence.
/// Follows the Single Responsibility Principle by focusing only on user profile management.
/// </summary>
public class UserProfileProvider : IUserProfileProvider
{
    private const string ProfileFileName = "user_profile.json";
    private UserProfile? _currentProfile;
    
    private class UserProfile
    {
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
    }
    
    public string? GetUserName()
    {
        return _currentProfile?.Name;
    }
    
    public void SetUserName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("User name cannot be null or empty.", nameof(name));
            
        if (_currentProfile == null)
        {
            CreateUserProfile(name);
        }
        else
        {
            _currentProfile.Name = name;
            _currentProfile.LastAccessed = DateTime.UtcNow;
            SaveUserProfile();
        }
    }
    
    public bool HasUserProfile()
    {
        return _currentProfile != null;
    }
    
    public void CreateUserProfile(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("User name cannot be null or empty.", nameof(name));
            
        _currentProfile = new UserProfile
        {
            Name = name,
            CreatedAt = DateTime.UtcNow,
            LastAccessed = DateTime.UtcNow
        };
        
        SaveUserProfile();
    }
    
    public void SaveUserProfile()
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
            Console.WriteLine($"Failed to save user profile: {ex.Message}");
        }
    }
    
    public void LoadUserProfile()
    {
        try
        {
            var profilePath = GetProfilePath();
            if (!File.Exists(profilePath))
                return;
                
            var json = File.ReadAllText(profilePath);
            _currentProfile = JsonSerializer.Deserialize<UserProfile>(json);
            
            if (_currentProfile != null)
            {
                _currentProfile.LastAccessed = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - profile loading is not critical for app functionality
            Console.WriteLine($"Failed to load user profile: {ex.Message}");
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