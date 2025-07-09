using System.Collections.Generic;
using StarConflictsRevolt.Clients.Models;
using StarConflictsRevolt.Server.Core;

namespace StarConflictsRevolt.Server.GameEngine;

public static class ChangeTracker
{
    public static List<GameObjectUpdate> ComputeDeltas(World oldWorld, World newWorld)
    {
        // TODO: Implement JSON diff logic
        return new List<GameObjectUpdate>();
    }
} 