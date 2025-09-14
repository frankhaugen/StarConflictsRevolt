using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Tests.ClientTests.UITests.TestData;

/// <summary>
/// Factory for creating test data for UI tests
/// </summary>
public static class TestDataFactory
{
    public static SessionDto CreateTestSession(string name = "Test Session", string type = "SinglePlayer")
    {
        return new SessionDto
        {
            Id = Guid.NewGuid(),
            SessionName = name,
            SessionType = type,
            Created = DateTime.UtcNow,
            IsActive = true
        };
    }
    
    public static List<SessionDto> CreateTestSessions(int count = 3)
    {
        var sessions = new List<SessionDto>();
        for (int i = 0; i < count; i++)
        {
            sessions.Add(CreateTestSession($"Test Session {i + 1}", i % 2 == 0 ? "SinglePlayer" : "Multiplayer"));
        }
        return sessions;
    }
    
    public static WorldDto CreateTestWorld()
    {
        return new WorldDto
        {
            Galaxy = new GalaxyDto
            {
                StarSystems = new List<StarSystemDto>
                {
                    new StarSystemDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Test System 1",
                        Coordinates = "1,1",
                        Planets = new List<PlanetDto>
                        {
                            new PlanetDto
                            {
                                Id = Guid.NewGuid(),
                                Name = "Test Planet 1",
                                Radius = 1000,
                                Mass = 1000000,
                                RotationSpeed = 1.0,
                                OrbitSpeed = 0.1,
                                DistanceFromSun = 100000
                            }
                        }
                    }
                }
            }
        };
    }
}
