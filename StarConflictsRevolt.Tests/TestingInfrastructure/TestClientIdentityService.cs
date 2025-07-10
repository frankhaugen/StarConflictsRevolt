using StarConflictsRevolt.Clients.Raylib.Services;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public class TestClientIdentityService : IClientIdentityService
{
    public string GetOrCreateClientId() => "test-client-id";
    public UserProfile GetUserProfile() => new UserProfile
    {
        UserId = "test-user",
        DisplayName = "Test User",
        UserName = "testuser"
    };
}