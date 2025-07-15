using StarConflictsRevolt.Clients.Models;

namespace StarConflictsRevolt.Clients.Raylib.Rendering.Core;

public class StarSystemWithSector
{
    public StarSystemDto System { get; set; }
    public int SectorId { get; set; }
    public bool IsVisible { get; set; }
    public string SectorName { get; set; } = string.Empty;
}