namespace StarConflictsRevolt.Clients.Raylib.Services;

public interface IClientIdentityService
{
    string GetOrCreateClientId();
    UserProfile GetUserProfile();
}