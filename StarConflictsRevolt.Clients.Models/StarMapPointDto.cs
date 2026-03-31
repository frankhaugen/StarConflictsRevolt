using System.Numerics;

namespace StarConflictsRevolt.Clients.Models;

/// <summary>A single point on the 2D star map (real-galaxy data, discified).</summary>
public record StarMapPointDto(string Name, Vector2 Coordinates);
