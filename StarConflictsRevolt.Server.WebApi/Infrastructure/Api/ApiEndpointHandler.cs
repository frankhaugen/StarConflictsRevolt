using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Api;

/// <summary>
/// Main API endpoint handler that orchestrates all sub-handlers
/// </summary>
public static class ApiEndpointHandler
{
    /// <summary>
    /// Maps all API endpoints using the modular handler system
    /// </summary>
    public static void MapAllEndpoints(WebApplication app)
    {
        // Health and status endpoints
        HealthEndpointHandler.MapEndpoints(app);
        
        // Authentication endpoints
        AuthEndpointHandler.MapEndpoints(app);
        
        // Session management endpoints
        SessionEndpointHandler.MapEndpoints(app);
        
        // Game action endpoints
        GameActionEndpointHandler.MapEndpoints(app);
        
        // Leaderboard endpoints
        LeaderboardEndpointHandler.MapEndpoints(app);
        
        // Event and snapshot endpoints
        EventEndpointHandler.MapEndpoints(app);
    }
} 