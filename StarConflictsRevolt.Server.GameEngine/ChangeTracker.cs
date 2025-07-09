using System.Collections.Generic;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.Core;

namespace StarConflictsRevolt.Server.GameEngine;

public static class ChangeTracker
{
    public static List<GameObjectUpdate> ComputeDeltas(World oldWorld, World newWorld)
    {
        var updates = new List<GameObjectUpdate>();
        if (!Equals(oldWorld, newWorld))
        {
            // For now, just emit a Changed update for the world root
            updates.Add(new GameObjectUpdate(newWorld.Id, UpdateType.Changed, null));
        }
        return updates;
    }
} 