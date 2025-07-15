namespace StarConflictsRevolt.Clients.Raylib.Infrastructure.Authentication;

public interface IClientIdentityService
{
    string GetOrCreateClientId();
}