using StarConflictsRevolt.Clients.Shared.Authentication;

namespace StarConflictsRevolt.Clients.Shared.User;

public interface IUserProfileProvider
{
    IUserProfile GetUserProfile();
}