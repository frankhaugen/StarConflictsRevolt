using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using StarConflictsRevolt.Clients.Models.Authentication;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;
using StarConflictsRevolt.Server.WebApi.Infrastructure.Configuration;
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

                var existingSessions = new List<object>();
                try
                {
                    var gamePersistence = context.RequestServices.GetRequiredService<IGamePersistence>();
                    var sessions = await gamePersistence.ListActiveSessionsAsync(context.RequestAborted);
                    existingSessions = sessions.Take(5).Select(s => new { s.Id, s.SessionName, s.Created, s.SessionType }).Cast<object>().ToList();
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

            var persistence = context.RequestServices.GetRequiredService<IGamePersistence>();
            Client? existingClient = await persistence.GetClientAsync(request.ClientId, context.RequestAborted);
            var nowSeen = DateTime.UtcNow;
            if (existingClient == null)
            {
                existingClient = new Client { Id = request.ClientId, LastSeen = nowSeen };
                logger.LogInformation("Created new client record for {ClientId}", request.ClientId);
            }
            else
            {
                existingClient.LastSeen = nowSeen;
                logger.LogDebug("Updated last seen timestamp for existing client {ClientId}", request.ClientId);
            }
            await persistence.UpsertClientAsync(existingClient, context.RequestAborted);

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