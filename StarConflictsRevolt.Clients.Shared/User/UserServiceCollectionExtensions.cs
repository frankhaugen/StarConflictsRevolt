using Microsoft.Extensions.DependencyInjection;

namespace StarConflictsRevolt.Clients.Shared;

public static class UserServiceCollectionExtensions
{
    /// <summary>
    /// Adds the user service to the service collection.
    /// </summary>
    /// <example>
    /// <code>csharp
    /// using StarConflictsRevolt.Clients.Shared;
    ///
    /// var services = new ServiceCollection();
    /// services.AddUserService();
    /// /// // Now you can resolve IUserProfileProvider and UserProfile from the service provider
    /// var serviceProvider = services.BuildServiceProvider();
    /// var userProfileProvider = serviceProvider.GetRequiredService[IUserProfileProvider]();
    /// var userProfile = serviceProvider.GetRequiredService[UserProfile]();
    /// // Use userProfileProvider and userProfile as needed
    /// </code>
    /// </example>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddUserService(this IServiceCollection services)
    {
        services.AddSingleton<IUserProfileProvider, UserProfileProvider>();
        services.AddSingleton<UserProfile>(sp => sp.GetRequiredService<IUserProfileProvider>().GetUserProfile());
        return services;
    }
}