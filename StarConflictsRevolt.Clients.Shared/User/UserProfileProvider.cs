using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace StarConflictsRevolt.Clients.Shared.User;

public class UserProfileProvider : IUserProfileProvider
{
    private UserProfile? _userProfile;
    
    public UserProfile GetUserProfile() => _userProfile ??= GetUserProfileInternal();

    private static UserProfile GetUserProfileInternal()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetWindowsUserProfile();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return GetLinuxUserProfile();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Implement macOS user profile retrieval if needed
            return new UserProfile
            {
                UserId = Guid.Empty.ToString(),
                DisplayName = "macOS User",
                UserName = Environment.UserName
            };
        }

        // Fallback for unsupported platforms
        return new UserProfile
        {
            UserId = Guid.Empty.ToString(),
            DisplayName = "Unknown User",
            UserName = "Unknown"
        };
    }

    [SupportedOSPlatform("linux")]
    private static UserProfile GetLinuxUserProfile()
    {
        try
        {
            // Get the current Linux user
            var name = Environment.UserName;
            if (string.IsNullOrEmpty(name))
            {
                name = "Unknown User";
            }
            var userId = Environment.GetEnvironmentVariable("USER") ?? name;
            var displayName = name; // Fallback to username if no display name is available

            return new UserProfile
            {
                UserId = userId,
                DisplayName = displayName,
                UserName = name
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get Linux user: {ex.Message}");
            return new UserProfile
            {
                UserId = Guid.Empty.ToString(),
                DisplayName = "Unknown User",
                UserName = "Unknown"
            };
        }
    }

    [SupportedOSPlatform("windows")]
    private static UserProfile GetWindowsUserProfile()
    {
        try
        {
            // Get the current Windows identity
            var identity = WindowsIdentity.GetCurrent();
            var name = identity?.Name ?? "Unknown User";

            // Try to get display name from Windows
            var displayName = name;
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var userPrincipal = UserPrincipal.Current;
                    if (userPrincipal != null) displayName = userPrincipal.DisplayName ?? userPrincipal.Name ?? name;
                }
            }
            catch (Exception ex)
            {
                // Fallback to username if display name lookup fails
                displayName = name;
                Console.WriteLine($"Failed to get display name: {ex.Message}");
            }

            return new UserProfile
            {
                UserId = identity?.User?.Value ?? Guid.NewGuid().ToString(),
                DisplayName = displayName,
                UserName = name
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get Windows identity: {ex.Message}");
            return new UserProfile
            {
                UserId = Guid.NewGuid().ToString(),
                DisplayName = "Unknown User",
                UserName = "Unknown"
            };
        }
    }
}