namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class CaptureResult
{
    public bool PlanetCaptured { get; set; }
    public Guid? NewOwnerId { get; set; }
    public int ResistanceLevel { get; set; } = 0; // 0-100, affects future stability
    public int LoyaltyChange { get; set; } = 0; // -100 to +100
    public List<string> CaptureEvents { get; set; } = new();
    public string FinalNarrative { get; set; } = string.Empty;
}