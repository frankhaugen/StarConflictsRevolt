using System.Text.Json;
using StarConflictsRevolt.Clients.Http.Http;
using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Http.TODO.Shared.Communication;

/// <summary>
/// Shared Game API extensions that can be used by both Raylib and Bliss clients.
/// This eliminates duplication between client projects.
/// </summary>
public static class GameApiExtensions
{
    /// <summary>
    ///     Creates a new game session
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="sessionName">Name of the session</param>
    /// <param name="sessionType">Type of session (SinglePlayer/Multiplayer)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session response with session ID and world data</returns>
    public static async Task<SessionResponse?> CreateNewSessionAsync(
        this IHttpApiClient client,
        string sessionName,
        string sessionType = "Multiplayer",
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            SessionName = sessionName,
            SessionType = sessionType
        };

        var response = await client.PostAsync("/game/session", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            Console.WriteLine($"Session creation response JSON: {responseJson}");

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var sessionResponse = JsonSerializer.Deserialize<SessionResponse>(responseJson, jsonOptions);
            Console.WriteLine($"Deserialized SessionResponse: SessionId={sessionResponse?.SessionId}, World={sessionResponse?.World != null}");

            return sessionResponse;
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine($"Session creation failed: {response.StatusCode} - {errorContent}");

        return null;
    }

    /// <summary>
    ///     Joins an existing game session
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="sessionId">Session ID to join</param>
    /// <param name="playerName">Player name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session response with session ID and world data</returns>
    public static async Task<SessionResponse?> JoinSessionAsync(
        this IHttpApiClient client,
        Guid sessionId,
        string playerName,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            PlayerName = playerName
        };

        var response = await client.PostAsync($"/game/session/{sessionId}/join", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Deserialize<SessionResponse>(responseJson, jsonOptions);
        }

        return null;
    }

    /// <summary>
    ///     Gets the current world state
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current world data</returns>
    public static async Task<WorldDto?> GetWorldStateAsync(
        this IHttpApiClient client,
        CancellationToken cancellationToken = default)
    {
        return await client.GetAsync<WorldDto>("/game/state", cancellationToken);
    }

    /// <summary>
    ///     Gets a list of available sessions
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available sessions</returns>
    public static async Task<List<SessionInfo>?> GetSessionsAsync(
        this IHttpApiClient client,
        CancellationToken cancellationToken = default)
    {
        return await client.GetAsync<List<SessionInfo>>("/game/sessions", cancellationToken);
    }

    /// <summary>
    ///     Moves a fleet from one planet to another
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="fleetId">Fleet ID to move</param>
    /// <param name="fromPlanetId">Source planet ID</param>
    /// <param name="toPlanetId">Destination planet ID</param>
    /// <param name="worldId">World ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    public static async Task<bool> MoveFleetAsync(
        this IHttpApiClient client,
        Guid fleetId,
        Guid fromPlanetId,
        Guid toPlanetId,
        Guid? worldId = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            FleetId = fleetId,
            FromPlanetId = fromPlanetId,
            ToPlanetId = toPlanetId
        };

        var uri = worldId.HasValue ? $"/game/move-fleet?worldId={worldId}" : "/game/move-fleet";
        var response = await client.PostAsync(uri, payload, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    ///     Builds a structure on a planet
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="planetId">Planet ID to build on</param>
    /// <param name="structureType">Type of structure to build</param>
    /// <param name="worldId">World ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    public static async Task<bool> BuildStructureAsync(
        this IHttpApiClient client,
        Guid planetId,
        string structureType,
        Guid? worldId = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            PlanetId = planetId,
            StructureType = structureType
        };

        var uri = worldId.HasValue ? $"/game/build-structure?worldId={worldId}" : "/game/build-structure";
        var response = await client.PostAsync(uri, payload, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    ///     Initiates an attack between fleets
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="attackerFleetId">Attacking fleet ID</param>
    /// <param name="defenderFleetId">Defending fleet ID</param>
    /// <param name="locationPlanetId">Planet where the attack occurs</param>
    /// <param name="worldId">World ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    public static async Task<bool> AttackAsync(
        this IHttpApiClient client,
        Guid attackerFleetId,
        Guid defenderFleetId,
        Guid locationPlanetId,
        Guid? worldId = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            AttackerFleetId = attackerFleetId,
            DefenderFleetId = defenderFleetId,
            LocationPlanetId = locationPlanetId
        };

        var uri = worldId.HasValue ? $"/game/attack?worldId={worldId}" : "/game/attack";
        var response = await client.PostAsync(uri, payload, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    ///     Initiates a diplomacy action
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="targetPlayerId">Target player ID</param>
    /// <param name="proposalType">Type of diplomacy proposal</param>
    /// <param name="message">Diplomacy message</param>
    /// <param name="worldId">World ID (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    public static async Task<bool> DiplomacyAsync(
        this IHttpApiClient client,
        Guid targetPlayerId,
        string proposalType,
        string message,
        Guid? worldId = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            TargetPlayerId = targetPlayerId,
            ProposalType = proposalType,
            Message = message
        };

        var uri = worldId.HasValue ? $"/game/diplomacy?worldId={worldId}" : "/game/diplomacy";
        var response = await client.PostAsync(uri, payload, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    ///     Gets the leaderboard for a session
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="sessionId">Session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Leaderboard data</returns>
    public static async Task<LeaderboardDto?> GetLeaderboardAsync(
        this IHttpApiClient client,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await client.GetAsync<LeaderboardDto>($"/game/leaderboard/{sessionId}", cancellationToken);
    }

    /// <summary>
    ///     Gets player statistics
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="sessionId">Session ID</param>
    /// <param name="playerId">Player ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Player statistics</returns>
    public static async Task<PlayerStatsDto?> GetPlayerStatsAsync(
        this IHttpApiClient client,
        Guid sessionId,
        Guid playerId,
        CancellationToken cancellationToken = default)
    {
        return await client.GetAsync<PlayerStatsDto>($"/game/player-stats/{sessionId}/{playerId}", cancellationToken);
    }

    /// <summary>
    ///     Gets top players
    /// </summary>
    /// <param name="client">The HTTP API client</param>
    /// <param name="count">Number of top players to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top players data</returns>
    public static async Task<TopPlayersDto?> GetTopPlayersAsync(
        this IHttpApiClient client,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        return await client.GetAsync<TopPlayersDto>($"/game/top-players?count={count}", cancellationToken);
    }
} 