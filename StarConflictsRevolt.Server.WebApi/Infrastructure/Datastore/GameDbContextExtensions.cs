﻿using Microsoft.EntityFrameworkCore;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Sessions;
using Galaxy = StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay.Galaxy;
using Planet = StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay.Planet;
using Session = StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay.Session;
using StarSystem = StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay.StarSystem;
using World = StarConflictsRevolt.Server.WebApi.Core.Domain.Gameplay.World;

namespace StarConflictsRevolt.Server.WebApi.Infrastructure.Datastore;

public static class GameDbContextExtensions
{
    public static async Task<World> GetWorldAsync(this GameDbContext context, Guid worldId, CancellationToken cancellationToken = default)
    {
        return await context.Worlds
                   .Include(w => w.Galaxy)
                   .ThenInclude(g => g.StarSystems)
                   .ThenInclude(s => s.Planets)
                   .FirstOrDefaultAsync(w => w.Id == worldId, cancellationToken)
               ?? throw new KeyNotFoundException($"World with ID {worldId} not found.");
    }

    public static async Task<Galaxy> GetGalaxyAsync(this GameDbContext context, Guid galaxyId, CancellationToken cancellationToken = default)
    {
        return await context.Galaxies
                   .Include(g => g.StarSystems)
                   .ThenInclude(s => s.Planets)
                   .FirstOrDefaultAsync(g => g.Id == galaxyId, cancellationToken)
               ?? throw new KeyNotFoundException($"Galaxy with ID {galaxyId} not found.");
    }

    public static async Task<StarSystem> GetStarSystemAsync(this GameDbContext context, Guid starSystemId, CancellationToken cancellationToken = default)
    {
        return await context.StarSystems
                   .Include(s => s.Planets)
                   .FirstOrDefaultAsync(s => s.Id == starSystemId, cancellationToken)
               ?? throw new KeyNotFoundException($"Star System with ID {starSystemId} not found.");
    }

    public static async Task<Planet> GetPlanetAsync(this GameDbContext context, Guid planetId, CancellationToken cancellationToken = default)
    {
        return await context.Planets
                   .FirstOrDefaultAsync(p => p.Id == planetId, cancellationToken)
               ?? throw new KeyNotFoundException($"Planet with ID {planetId} not found.");
    }

    public static async Task<Client> GetClientAsync(this GameDbContext context, string clientId, CancellationToken cancellationToken = default)
    {
        return await context.Clients
                   .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken)
               ?? throw new KeyNotFoundException($"Client with ID {clientId} not found.");
    }

    public static async Task<Guid> CreateSessionAsync(this GameDbContext context, string sessionName, SessionType sessionType, CancellationToken cancellationToken = default)
    {
        var session = new Session
        {
            Id = IGameObject.CreateId(),
            SessionName = sessionName,
            Created = DateTime.UtcNow,
            IsActive = true,
            SessionType = sessionType
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

    public static async Task<Session?> GetSessionAsync(this GameDbContext context, Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public static async Task<List<Session>> GetActiveSessionsAsync(this GameDbContext context, string clientId, CancellationToken cancellationToken = default)
    {
        return await context.Sessions
            .Where(s => s.IsActive)
            .Where(s => s.ClientId == clientId)
            .ToListAsync(cancellationToken);
    }
}