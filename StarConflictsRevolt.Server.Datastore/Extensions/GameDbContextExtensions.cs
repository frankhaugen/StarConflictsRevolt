using Microsoft.EntityFrameworkCore;
using StarConflictsRevolt.Server.Core;
using StarConflictsRevolt.Server.Core.Models;
using StarConflictsRevolt.Server.Datastore.Entities;
using Galaxy = StarConflictsRevolt.Server.Datastore.Entities.Galaxy;
using Planet = StarConflictsRevolt.Server.Datastore.Entities.Planet;
using Session = StarConflictsRevolt.Server.Datastore.Entities.Session;
using StarSystem = StarConflictsRevolt.Server.Datastore.Entities.StarSystem;
using World = StarConflictsRevolt.Server.Datastore.Entities.World;

namespace StarConflictsRevolt.Server.Datastore.Extensions;

public static class GameDbContextExtensions
{
    public static async Task<World> GetWorldAsync(this GameDbContext context, Guid worldId, CancellationToken cancellationToken = default) =>
        await context.Worlds
            .Include(w => w.Galaxy)
            .ThenInclude(g => g.StarSystems)
            .ThenInclude(s => s.Planets)
            .FirstOrDefaultAsync(w => w.Id == worldId, cancellationToken)
        ?? throw new KeyNotFoundException($"World with ID {worldId} not found.");

    public static async Task<Galaxy> GetGalaxyAsync(this GameDbContext context, Guid galaxyId, CancellationToken cancellationToken = default) =>
        await context.Galaxies
            .Include(g => g.StarSystems)
            .ThenInclude(s => s.Planets)
            .FirstOrDefaultAsync(g => g.Id == galaxyId, cancellationToken)
        ?? throw new KeyNotFoundException($"Galaxy with ID {galaxyId} not found.");

    public static async Task<StarSystem> GetStarSystemAsync(this GameDbContext context, Guid starSystemId, CancellationToken cancellationToken = default) =>
        await context.StarSystems
            .Include(s => s.Planets)
            .FirstOrDefaultAsync(s => s.Id == starSystemId, cancellationToken)
        ?? throw new KeyNotFoundException($"Star System with ID {starSystemId} not found.");

    public static async Task<Planet> GetPlanetAsync(this GameDbContext context, Guid planetId, CancellationToken cancellationToken = default) =>
        await context.Planets
            .FirstOrDefaultAsync(p => p.Id == planetId, cancellationToken)
        ?? throw new KeyNotFoundException($"Planet with ID {planetId} not found.");
    
    public static async Task<Guid> CreateSessionAsync(this GameDbContext context, string sessionName, CancellationToken cancellationToken = default)
    {
        var session = new Session()
        {
            Id = IGameObject.CreateId(),
            SessionName = sessionName,
            Created = DateTime.UtcNow,
            IsActive = true
        };
        
        var entityEntry = context.Sessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);
        
        if (entityEntry.Entity.Id != session.Id)
            throw new InvalidOperationException("Failed to create session: ID mismatch after save.");
        
        session.Id = entityEntry.Entity.Id;
        return session.Id;
    }
    
    public static async Task EndSessionAsync(this GameDbContext context, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await context.Sessions.FindAsync(new object[] { sessionId }, cancellationToken);
        if (session == null)
            throw new KeyNotFoundException($"Session with ID {sessionId} not found.");

        session.IsActive = false;
        session.Ended = DateTime.UtcNow;

        context.Sessions.Update(session);
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public static async Task<Session?> GetSessionAsync(this GameDbContext context, Guid sessionId, CancellationToken cancellationToken = default) =>
        await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    
    public static async Task<List<Session>> GetActiveSessionsAsync(this GameDbContext context, CancellationToken cancellationToken = default) =>
        await context.Sessions
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);
}