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
        return new WorldDto(
            Guid.NewGuid(),
            new GalaxyDto(
                Guid.NewGuid(),
                new List<StarSystemDto>
                {
                    new StarSystemDto(
                        Guid.NewGuid(),
                        "Test System 1",
                        new List<PlanetDto>
                        {
                            new PlanetDto(
                                Guid.NewGuid(),
                                "Test Planet 1",
                                1000,
                                1000000,
                                1.0,
                                0.1,
                                100000
                            )
                        },
                        new System.Numerics.Vector2(1, 1)
                    )
                }
            ),
            null
        );
    }
}
