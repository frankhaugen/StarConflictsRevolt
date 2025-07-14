using StarConflictsRevolt.Clients.Raylib.Game.User;

namespace StarConflictsRevolt.Clients.Raylib.Infrastructure.Authentication;

public interface IClientIdentityService
{
    string GetOrCreateClientId();
    UserProfile GetUserProfile();
}