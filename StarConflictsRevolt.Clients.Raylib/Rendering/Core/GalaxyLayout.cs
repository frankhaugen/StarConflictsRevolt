using System.Numerics;
using StarConflictsRevolt.Clients.Models;
using System.Collections.Generic;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

public class SectorInfo
{
    public int SectorId { get; set; }
    public bool IsVisible { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class StarSystemWithSector
{
    public StarSystemDto System { get; set; }
    public int SectorId { get; set; }
    public bool IsVisible { get; set; }
    public string SectorName { get; set; } = string.Empty;
}

public static class GalaxyLayout
{
    private static List<SectorInfo> _sectors = new()
    {
        new SectorInfo { SectorId = 1, IsVisible = true, Name = "Core Worlds" },
        new SectorInfo { SectorId = 2, IsVisible = true, Name = "Mid Rim" },
        new SectorInfo { SectorId = 3, IsVisible = false, Name = "Outer Rim" },
        new SectorInfo { SectorId = 4, IsVisible = false, Name = "Unknown Regions" },
    };

    private static List<StarSystemWithSector> _systems = new()
    {
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Coruscant", new List<PlanetDto>(), new Vector2(200, 300)), SectorId = 1, IsVisible = true, SectorName = "Core Worlds" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Alderaan", new List<PlanetDto>(), new Vector2(350, 250)), SectorId = 1, IsVisible = true, SectorName = "Core Worlds" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Corellia", new List<PlanetDto>(), new Vector2(180, 420)), SectorId = 1, IsVisible = true, SectorName = "Core Worlds" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Kuat", new List<PlanetDto>(), new Vector2(270, 380)), SectorId = 1, IsVisible = true, SectorName = "Core Worlds" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Bespin", new List<PlanetDto>(), new Vector2(500, 200)), SectorId = 2, IsVisible = true, SectorName = "Mid Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Naboo", new List<PlanetDto>(), new Vector2(600, 350)), SectorId = 2, IsVisible = true, SectorName = "Mid Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Ryloth", new List<PlanetDto>(), new Vector2(700, 250)), SectorId = 2, IsVisible = true, SectorName = "Mid Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Muunilinst", new List<PlanetDto>(), new Vector2(800, 400)), SectorId = 2, IsVisible = true, SectorName = "Mid Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Tatooine", new List<PlanetDto>(), new Vector2(950, 500)), SectorId = 3, IsVisible = false, SectorName = "Outer Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Geonosis", new List<PlanetDto>(), new Vector2(1100, 600)), SectorId = 3, IsVisible = false, SectorName = "Outer Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Hoth", new List<PlanetDto>(), new Vector2(1200, 400)), SectorId = 3, IsVisible = false, SectorName = "Outer Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Dagobah", new List<PlanetDto>(), new Vector2(1000, 300)), SectorId = 3, IsVisible = false, SectorName = "Outer Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Kamino", new List<PlanetDto>(), new Vector2(1300, 200)), SectorId = 4, IsVisible = false, SectorName = "Unknown Regions" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Ilum", new List<PlanetDto>(), new Vector2(1400, 350)), SectorId = 4, IsVisible = false, SectorName = "Unknown Regions" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Jakku", new List<PlanetDto>(), new Vector2(1500, 500)), SectorId = 4, IsVisible = false, SectorName = "Unknown Regions" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Exegol", new List<PlanetDto>(), new Vector2(1600, 600)), SectorId = 4, IsVisible = false, SectorName = "Unknown Regions" },
        // Add more systems for a total of 24...
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Dantooine", new List<PlanetDto>(), new Vector2(400, 600)), SectorId = 1, IsVisible = true, SectorName = "Core Worlds" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Ord Mantell", new List<PlanetDto>(), new Vector2(500, 700)), SectorId = 2, IsVisible = true, SectorName = "Mid Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Sullust", new List<PlanetDto>(), new Vector2(600, 800)), SectorId = 2, IsVisible = true, SectorName = "Mid Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Mon Cala", new List<PlanetDto>(), new Vector2(700, 900)), SectorId = 2, IsVisible = true, SectorName = "Mid Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Felucia", new List<PlanetDto>(), new Vector2(800, 1000)), SectorId = 3, IsVisible = false, SectorName = "Outer Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Mandalore", new List<PlanetDto>(), new Vector2(900, 1100)), SectorId = 3, IsVisible = false, SectorName = "Outer Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Lothal", new List<PlanetDto>(), new Vector2(1000, 1200)), SectorId = 3, IsVisible = false, SectorName = "Outer Rim" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Scarif", new List<PlanetDto>(), new Vector2(1100, 1300)), SectorId = 4, IsVisible = false, SectorName = "Unknown Regions" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Jedha", new List<PlanetDto>(), new Vector2(1200, 1400)), SectorId = 4, IsVisible = false, SectorName = "Unknown Regions" },
        new StarSystemWithSector { System = new StarSystemDto(Guid.NewGuid(), "Mustafar", new List<PlanetDto>(), new Vector2(1300, 1500)), SectorId = 4, IsVisible = false, SectorName = "Unknown Regions" },
    };

    public static GalaxyDto GetGalaxy()
    {
        // Only include visible systems
        var visibleSystems = new List<StarSystemDto>();
        foreach (var sys in _systems)
        {
            var sector = _sectors.Find(s => s.SectorId == sys.SectorId);
            if (sector != null && sector.IsVisible)
            {
                visibleSystems.Add(sys.System);
            }
        }
        return new GalaxyDto(Guid.NewGuid(), visibleSystems);
    }

    public static void ToggleSectorVisibility(int sectorId)
    {
        var sector = _sectors.Find(s => s.SectorId == sectorId);
        if (sector != null)
        {
            sector.IsVisible = !sector.IsVisible;
            // Update all systems in this sector
            foreach (var sys in _systems)
            {
                if (sys.SectorId == sectorId)
                {
                    sys.IsVisible = sector.IsVisible;
                }
            }
        }
    }

    public static IEnumerable<SectorInfo> GetSectors() => _sectors;
    public static IEnumerable<StarSystemWithSector> GetAllSystems() => _systems;
} 