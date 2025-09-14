using StarConflictsRevolt.Clients.Blazor.Services;

namespace StarConflictsRevolt.Tests.ClientTests.UnitTests;

/// <summary>
/// Unit tests for TelemetryService
/// </summary>
public class TelemetryServiceTests
{
    private TelemetryService _telemetryService = null!;

    [SetUp]
    public void Setup()
    {
        _telemetryService = new TelemetryService();
    }

    [TearDown]
    public void TearDown()
    {
        _telemetryService?.Dispose();
    }

    [Test]
    public void RecordSignalRMessage_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.RecordSignalRMessage());
    }

    [Test]
    public void RecordHttpRequest_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.RecordHttpRequest());
    }

    [Test]
    public void RecordHttpError_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.RecordHttpError());
    }

    [Test]
    public void RecordHttpResponseTime_ValidTime_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.RecordHttpResponseTime(1.5));
    }

    [Test]
    public void RecordGameAction_ValidAction_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.RecordGameAction("test_action"));
    }

    [Test]
    public void UpdateActiveConnections_ValidCount_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.UpdateActiveConnections(5));
    }

    [Test]
    public void UpdateMemoryUsage_ValidBytes_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.UpdateMemoryUsage(1024 * 1024));
    }

    [Test]
    public void Dispose_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => _telemetryService.Dispose());
    }

    [Test]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.Dispose();
        Assert.DoesNotThrow(() => _telemetryService.Dispose());
    }
}
