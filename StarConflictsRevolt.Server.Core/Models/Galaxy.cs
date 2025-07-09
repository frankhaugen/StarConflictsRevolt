using System.Collections.Generic;

namespace StarConflictsRevolt.Server.Core.Models;

public record Galaxy(
    Guid Id,
    List<StarSystem> StarSystems
);