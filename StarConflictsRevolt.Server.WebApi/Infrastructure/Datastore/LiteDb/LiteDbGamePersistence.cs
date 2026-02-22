using LiteDB;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;
using Session = StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay.Session;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore.LiteDb;

public sealed class LiteDbGamePersistence : IGamePersistence
{
    private readonly ILiteDatabase _db;
    private const string SessionsCollection = "sessions";
    private const string ClientsCollection = "clients";
    private const string PlayerStatsCollection = "playerstats";

    public LiteDbGamePersistence(ILiteDatabase db)
    {
        _db = db;
    }

    public Task<Guid> CreateSessionAsync(string sessionName, SessionType sessionType, string? playerId, CancellationToken cancellationToken = default)
    {
        var session = new Session
        {
            Id = Guid.NewGuid(),
            SessionName = sessionName,
            Created = DateTime.UtcNow,
            IsActive = true,
            SessionType = sessionType,
            ClientId = null,
            PlayerId = playerId
        };
        var col = _db.GetCollection<SessionDoc>(SessionsCollection);
        col.Insert(SessionDoc.From(session));
        return Task.FromResult(session.Id);
    }

    public async Task EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var col = _db.GetCollection<SessionDoc>(SessionsCollection);
        var doc = col.FindById(sessionId);
        if (doc == null) return;
        doc.IsActive = false;
        doc.Ended = DateTime.UtcNow;
        col.Update(doc);
    }

    public Task<Session?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var col = _db.GetCollection<SessionDoc>(SessionsCollection);
        var doc = col.FindById(sessionId);
        return Task.FromResult<Session?>(doc?.ToSession());
    }

    public Task<List<Session>> GetActiveSessionsByPlayerAsync(string playerId, CancellationToken cancellationToken = default)
    {
        var col = _db.GetCollection<SessionDoc>(SessionsCollection);
        var list = col.Find(s => s.IsActive && s.PlayerId == playerId).Select(s => s.ToSession()).ToList();
        return Task.FromResult(list);
    }

    public Task<List<Session>> ListActiveSessionsAsync(CancellationToken cancellationToken = default)
    {
        var col = _db.GetCollection<SessionDoc>(SessionsCollection);
        var list = col.Find(s => s.IsActive).OrderByDescending(s => s.Created).Select(s => s.ToSession()).ToList();
        return Task.FromResult(list);
    }

    public Task<Client?> GetClientAsync(string clientId, CancellationToken cancellationToken = default)
    {
        var col = _db.GetCollection<ClientDoc>(ClientsCollection);
        var doc = col.FindById(clientId);
        return Task.FromResult<Client?>(doc?.ToClient());
    }

    public Task UpsertClientAsync(Client client, CancellationToken cancellationToken = default)
    {
        var col = _db.GetCollection<ClientDoc>(ClientsCollection);
        col.Upsert(ClientDoc.From(client));
        return Task.CompletedTask;
    }

    public Task<List<PlayerStats>> GetLeaderboardAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var col = _db.GetCollection<PlayerStatsDoc>(PlayerStatsCollection);
        var list = col.Find(ps => ps.SessionId == sessionId)
            .OrderByDescending(ps => ps.BattlesWon)
            .ThenByDescending(ps => ps.PlanetsControlled)
            .ThenByDescending(ps => ps.FleetsOwned)
            .Select(ps => ps.ToPlayerStats())
            .ToList();
        return Task.FromResult(list);
    }

    public Task<PlayerStats?> GetPlayerStatsAsync(Guid sessionId, Guid playerId, CancellationToken cancellationToken = default)
    {
        var col = _db.GetCollection<PlayerStatsDoc>(PlayerStatsCollection);
        var doc = col.Find(ps => ps.SessionId == sessionId && ps.PlayerId == playerId).FirstOrDefault();
        return Task.FromResult<PlayerStats?>(doc?.ToPlayerStats());
    }

    public Task<List<PlayerStats>> GetTopPlayersAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var col = _db.GetCollection<PlayerStatsDoc>(PlayerStatsCollection);
        var list = col.FindAll()
            .OrderByDescending(ps => ps.BattlesWon)
            .ThenByDescending(ps => ps.PlanetsControlled)
            .Take(count)
            .Select(ps => ps.ToPlayerStats())
            .ToList();
        return Task.FromResult(list);
    }
}
