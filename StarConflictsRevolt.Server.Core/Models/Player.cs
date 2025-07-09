using System.Collections.Generic;

namespace StarConflictsRevolt.Server.Core.Models;

public record Player(
    Guid Id,
    string Name,
    PlayerController Controller,
    List<Fleet> Fleets
);