using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Security;

namespace StarConflictsRevolt.Server.WebApi.API.Handlers.Endpoints;

/// <summary>
///     Handles authentication and token endpoints
/// </summary>
public static class AuthEndpointHandler
{
    public static void MapEndpoints(WebApplication app)
    {
        // Token endpoint for client authentication
        app.MapPost("/token", async context =>
        {
            var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("TokenEndpoint");

            var request = await context.Request.ReadFromJsonAsync<TokenRequest>(context.RequestAborted);
            if (request == null || string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.ClientSecret))
            {
                logger.LogWarning("Invalid token request received: ClientId={ClientId}, HasSecret={HasSecret}",
                    request?.ClientId ?? "null", !string.IsNullOrEmpty(request?.ClientSecret));

                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "invalid_request",
                    error_description = "ClientId and ClientSecret are required",
                    timestamp = DateTime.UtcNow
                }, context.RequestAborted);
                return;
            }

            logger.LogInformation("Token request received for client {ClientId}", request.ClientId);

            // The only valid failure case: wrong secret
            if (request.ClientSecret != Constants.Secret)
            {
                logger.LogWarning("Authentication failed for client {ClientId}: Invalid secret provided", request.ClientId);

                // Get existing sessions to provide helpful information
                var dbContext = context.RequestServices.GetRequiredService<GameDbContext>();
                var existingSessions = new List<object>();

                try
                {
                    var sessions = await dbContext.Sessions
                        .Where(s => s.IsActive)
                        .OrderByDescending(s => s.Created)
                        .Take(5) // Limit to 5 most recent sessions
                        .Select(s => new
                        {
                            s.Id,
                            s.SessionName,
                            s.Created,
                            s.SessionType
                        })
                        .ToListAsync(context.RequestAborted);

                    existingSessions = sessions.Cast<object>().ToList();
                }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Could not retrieve existing sessions for error response");
                }

                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "invalid_client",
                    error_description = "Invalid client secret. Please check your configuration.",
                    client_id = request.ClientId,
                    timestamp = DateTime.UtcNow,
                    existing_sessions = existingSessions,
                    session_count = existingSessions.Count
                }, context.RequestAborted);
                return;
            }

            var gameDbContext = context.RequestServices.GetRequiredService<GameDbContext>();

            // Check if database is ready
            try
            {
                var canConnect = await gameDbContext.Database.CanConnectAsync(context.RequestAborted);
                if (!canConnect)
                {
                    logger.LogError("Database connection failed for client {ClientId}", request.ClientId);
                    context.Response.StatusCode = 503;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "service_unavailable",
                        error_description = "Database is not ready. Please try again later.",
                        timestamp = DateTime.UtcNow
                    }, context.RequestAborted);
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Database connection check failed for client {ClientId}", request.ClientId);
                context.Response.StatusCode = 503;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "service_unavailable",
                    error_description = "Database connection failed. Please try again later.",
                    timestamp = DateTime.UtcNow
                }, context.RequestAborted);
                return;
            }

            // Use direct lookup if GetClientAsync is not available
            Client? existingClient = null;
            try
            {
                existingClient = gameDbContext.Clients.FirstOrDefault(c => c.Id == request.ClientId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to query Clients table for client {ClientId}. Database may not be fully initialized.", request.ClientId);
                context.Response.StatusCode = 503;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "service_unavailable",
                    error_description = "Database is not fully initialized. Please try again later.",
                    timestamp = DateTime.UtcNow
                }, context.RequestAborted);
                return;
            }

            if (existingClient == null)
            {
                try
                {
                    existingClient = new Client { Id = request.ClientId, LastSeen = DateTime.UtcNow };
                    gameDbContext.Clients.Add(existingClient);
                    await gameDbContext.SaveChangesAsync(context.RequestAborted);
                    logger.LogInformation("Created new client record for {ClientId}", request.ClientId);
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    // Handle duplicate key constraint violation
                    logger.LogWarning("Client {ClientId} already exists, attempting to update instead", request.ClientId);
                    
                    // Try to find the existing client again
                    existingClient = gameDbContext.Clients.FirstOrDefault(c => c.Id == request.ClientId);
                    if (existingClient != null)
                    {
                        existingClient.LastSeen = DateTime.UtcNow;
                        gameDbContext.Clients.Update(existingClient);
                        await gameDbContext.SaveChangesAsync(context.RequestAborted);
                        logger.LogInformation("Updated existing client record for {ClientId} after duplicate key error", request.ClientId);
                    }
                    else
                    {
                        logger.LogError("Could not find or create client {ClientId} after duplicate key error", request.ClientId);
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "server_error",
                            error_description = "Failed to create or update client record",
                            timestamp = DateTime.UtcNow
                        }, context.RequestAborted);
                        return;
                    }
                }
            }
            else
            {
                existingClient.LastSeen = DateTime.UtcNow;
                gameDbContext.Clients.Update(existingClient);
                await gameDbContext.SaveChangesAsync(context.RequestAborted);
                logger.LogDebug("Updated last seen timestamp for existing client {ClientId}", request.ClientId);
            }

            var claims = new[] { new Claim("client_id", request.ClientId) };
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                JwtConfig.Issuer,
                JwtConfig.Audience,
                claims,
                now.AddMinutes(-5),
                now.AddHours(1),
                new SigningCredentials(
                    JwtConfig.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);

            logger.LogInformation("Successfully issued token for client {ClientId}", request.ClientId);

            // Return JSON payload consistent with OAuth conventions and test expectations
            await context.Response.WriteAsJsonAsync(
                new TokenResponse()
                {
                    AccessToken = tokenString,
                    TokenType = TokenType.Bearer,
                    ExpiresAt = now.AddHours(1)
                },
                context.RequestAborted);
        }).AllowAnonymous();
    }
}