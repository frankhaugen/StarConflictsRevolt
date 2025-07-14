using FluentAssertions;
using Microsoft.Extensions.Logging;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Resources;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Technology;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Victory;

namespace StarConflictsRevolt.Tests.ServerTests.UnitTests;

public class GameContentTests
{
    private readonly GameContentService _gameContentService = null!;

    public GameContentTests()
    {
        var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<GameContentService>();
        _gameContentService = new GameContentService(logger);
    }

    [Test]
    public void GetAllTechnologies_ReturnsAllTechnologies()
    {
        // Act
        var technologies = _gameContentService.GetAllTechnologies().ToList();

        // Assert
        technologies.Should().NotBeEmpty();
        technologies.Count.Should().BeGreaterThan(10);

        // Check that all technology categories are represented
        var categories = technologies.Select(t => t.Category).Distinct().ToList();
        categories.Should().Contain(TechnologyCategory.Combat);
        categories.Should().Contain(TechnologyCategory.Defense);
        categories.Should().Contain(TechnologyCategory.Propulsion);
        categories.Should().Contain(TechnologyCategory.Economy);
        categories.Should().Contain(TechnologyCategory.Industry);
    }

    [Test]
    public void GetTechnology_WithValidName_ReturnsTechnology()
    {
        // Act
        var technology = _gameContentService.GetTechnology("Basic Weapons");

        // Assert
        technology.Should().NotBeNull();
        technology!.Name.Should().Be("Basic Weapons");
        technology.Category.Should().Be(TechnologyCategory.Combat);
        technology.Level.Should().Be(1);
    }

    [Test]
    public void GetTechnology_WithInvalidName_ReturnsNull()
    {
        // Act
        var technology = _gameContentService.GetTechnology("Invalid Technology");

        // Assert
        technology.Should().BeNull();
    }

    [Test]
    public void GetAvailableTechnologies_WithNoResearch_ReturnsBasicTechnologies()
    {
        // Arrange
        var researchedTechnologies = new List<string>();

        // Act
        var available = _gameContentService.GetAvailableTechnologies(researchedTechnologies).ToList();

        // Assert
        available.Should().NotBeEmpty();

        // Should only include level 1 technologies (no prerequisites)
        foreach (var tech in available)
        {
            tech.Level.Should().Be(1);
            tech.Prerequisites.Should().BeEmpty();
        }
    }

    [Test]
    public void GetAvailableTechnologies_WithPrerequisites_ReturnsAdvancedTechnologies()
    {
        // Arrange
        var researchedTechnologies = new List<string> { "Basic Weapons" };

        // Act
        var available = _gameContentService.GetAvailableTechnologies(researchedTechnologies).ToList();

        // Assert
        available.Should().NotBeEmpty();

        // Should include Advanced Weapons
        var advancedWeapons = available.FirstOrDefault(t => t.Name == "Advanced Weapons");
        advancedWeapons.Should().NotBeNull();
        advancedWeapons!.Level.Should().Be(2);
    }

    [Test]
    public void CanResearchTechnology_WithValidConditions_ReturnsTrue()
    {
        // Arrange
        var researchedTechnologies = new List<string>();
        var availableCredits = 200;

        // Act
        var canResearch = _gameContentService.CanResearchTechnology("Basic Weapons", researchedTechnologies, availableCredits);

        // Assert
        canResearch.Should().BeTrue();
    }

    [Test]
    public void CanResearchTechnology_WithInsufficientCredits_ReturnsFalse()
    {
        // Arrange
        var researchedTechnologies = new List<string>();
        var availableCredits = 50; // Less than Basic Weapons cost (100)

        // Act
        var canResearch = _gameContentService.CanResearchTechnology("Basic Weapons", researchedTechnologies, availableCredits);

        // Assert
        canResearch.Should().BeFalse();
    }

    [Test]
    public void CanResearchTechnology_WithMissingPrerequisites_ReturnsFalse()
    {
        // Arrange
        var researchedTechnologies = new List<string>(); // No prerequisites researched
        var availableCredits = 500;

        // Act
        var canResearch = _gameContentService.CanResearchTechnology("Advanced Weapons", researchedTechnologies, availableCredits);

        // Assert
        canResearch.Should().BeFalse();
    }

    [Test]
    public void GetAllVictoryConditions_ReturnsAllVictoryConditions()
    {
        // Act
        var victoryConditions = _gameContentService.GetAllVictoryConditions().ToList();

        // Assert
        victoryConditions.Should().NotBeEmpty();
        victoryConditions.Count.Should().Be(5);

        // Check that all victory types are represented
        var types = victoryConditions.Select(v => v.Type).Distinct().ToList();
        types.Should().Contain(VictoryType.Military);
        types.Should().Contain(VictoryType.Economic);
        types.Should().Contain(VictoryType.Technology);
        types.Should().Contain(VictoryType.Time);
        types.Should().Contain(VictoryType.Diplomatic);
    }

    [Test]
    public void GetVictoryCondition_WithValidName_ReturnsVictoryCondition()
    {
        // Act
        var victoryCondition = _gameContentService.GetVictoryCondition("Military Victory");

        // Assert
        victoryCondition.Should().NotBeNull();
        victoryCondition!.Name.Should().Be("Military Victory");
        victoryCondition.Type.Should().Be(VictoryType.Military);
        victoryCondition.RequiredPlanetPercentage.Should().Be(75);
    }

    [Test]
    public void GetVictoryCondition_WithValidType_ReturnsVictoryCondition()
    {
        // Act
        var victoryCondition = _gameContentService.GetVictoryCondition(VictoryType.Economic);

        // Assert
        victoryCondition.Should().NotBeNull();
        victoryCondition!.Type.Should().Be(VictoryType.Economic);
        victoryCondition.RequiredCredits.Should().Be(10000);
    }

    [Test]
    public void GetResourceDefinitions_ReturnsAllResourceTypes()
    {
        // Act
        var resourceDefinitions = _gameContentService.GetResourceDefinitions();

        // Assert
        resourceDefinitions.Should().NotBeEmpty();
        resourceDefinitions.Count.Should().Be(4);

        // Check that all resource types are represented
        resourceDefinitions.Should().ContainKey(ResourceType.Credits);
        resourceDefinitions.Should().ContainKey(ResourceType.Materials);
        resourceDefinitions.Should().ContainKey(ResourceType.Fuel);
        resourceDefinitions.Should().ContainKey(ResourceType.Food);
    }

    [Test]
    public void GetResourceDefinition_WithValidType_ReturnsDefinition()
    {
        // Act
        var definition = _gameContentService.GetResourceDefinition(ResourceType.Credits);

        // Assert
        definition.Should().NotBeNull();
        definition!.Name.Should().Be("Credits");
        definition.BaseValue.Should().Be(1);
        definition.StorageLimit.Should().Be(10000);
    }

    [Test]
    public void TryConvertResource_WithValidConversion_ReturnsTrue()
    {
        // Act
        var success = _gameContentService.TryConvertResource(ResourceType.Materials, ResourceType.Credits, 10, out var convertedAmount);

        // Assert
        success.Should().BeTrue();
        convertedAmount.Should().Be(5); // 10 materials = 5 credits (0.5 rate)
    }

    [Test]
    public void TryConvertResource_WithSameType_ReturnsTrue()
    {
        // Act
        var success = _gameContentService.TryConvertResource(ResourceType.Credits, ResourceType.Credits, 10, out var convertedAmount);

        // Assert
        success.Should().BeTrue(); // Same type conversion should work
        convertedAmount.Should().Be(10);
    }

    [Test]
    public void GetResourceValue_WithValidType_ReturnsCorrectValue()
    {
        // Act
        var value = _gameContentService.GetResourceValue(ResourceType.Fuel, 10);

        // Assert
        value.Should().Be(30); // 10 fuel * 3 base value = 30
    }

    [Test]
    public void GetGameStatistics_ReturnsValidStatistics()
    {
        // Act
        var statistics = _gameContentService.GetGameStatistics();

        // Assert
        statistics.Should().NotBeEmpty();
        statistics["ShipTypes"].Should().BeOfType<int>().Which.Should().BeGreaterThan(0);
        statistics["StructureTypes"].Should().BeOfType<int>().Which.Should().BeGreaterThan(0);
        statistics["PlanetTypes"].Should().BeOfType<int>().Which.Should().BeGreaterThan(0);
        statistics["Technologies"].Should().BeOfType<int>().Which.Should().BeGreaterThan(0);
        statistics["VictoryConditions"].Should().BeOfType<int>().Which.Should().BeGreaterThan(0);
        statistics["ResourceTypes"].Should().BeOfType<int>().Which.Should().BeGreaterThan(0);
        statistics["TotalContentItems"].Should().BeOfType<int>().Which.Should().BeGreaterThan(0);
    }

    [Test]
    public void GetGameBalanceInfo_ReturnsComprehensiveInfo()
    {
        // Act
        var info = _gameContentService.GetGameBalanceInfo();

        // Assert
        info.Should().NotBeEmpty();
        info.Should().Contain("ship types");
        info.Should().Contain("structure types");
        info.Should().Contain("planet types");
        info.Should().Contain("technologies");
        info.Should().Contain("victory conditions");
        info.Should().Contain("resource types");
    }
}