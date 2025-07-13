using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace StarConflictsRevolt.Clients.Raylib.Services;

public record UserProfile
{
    public string UserId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;

    // Helper method to get Windows user profile
    public static UserProfile GetUserProfile()
    {
        try
        {
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