﻿namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Combat;

public class GroundCombatCinematicData
{
    public List<string> Highlights { get; set; } = new();
    public List<string> CriticalMoments { get; set; } = new();
    public string FinalNarrative { get; set; } = string.Empty;
    public Dictionary<string, object> CustomData { get; set; } = new();
}