namespace StarConflictsRevolt.Server.WebApi.Core.Domain.Resources;

public class ResourceDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double BaseValue { get; set; } // Value in credits
    public int StorageLimit { get; set; }
    public Dictionary<ResourceType, double> ConversionRates { get; set; } = new();
}