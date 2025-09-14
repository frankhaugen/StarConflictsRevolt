using StarConflictsRevolt.Clients.Blazor.Services;
using TUnit;

namespace StarConflictsRevolt.Tests.ClientTests.UnitTests;

/// <summary>
/// Unit tests for TelemetryService
/// </summary>
public class TelemetryServiceTests
{
    private TelemetryService _telemetryService = null!;

    public TelemetryServiceTests()
    {
        _telemetryService = new TelemetryService();
    }

    public void Dispose()
    {
        _telemetryService?.Dispose();
    }

    [Test]
    public void RecordSignalRMessage_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.RecordSignalRMessage();
    }

    [Test]
    public void RecordHttpRequest_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.RecordHttpRequest();
    }

    [Test]
    public void RecordHttpError_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.RecordHttpError();
    }

    [Test]
    public void RecordHttpResponseTime_ValidTime_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.RecordHttpResponseTime(1.5);
    }

    [Test]
    public void RecordGameAction_ValidAction_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.RecordGameAction("test_action");
    }

    [Test]
    public void UpdateActiveConnections_ValidCount_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.UpdateActiveConnections(5);
    }

    [Test]
    public void UpdateMemoryUsage_ValidBytes_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.UpdateMemoryUsage(1024 * 1024);
    }

    [Test]
    public void Dispose_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.Dispose();
    }

    [Test]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Act & Assert
        _telemetryService.Dispose();
        _telemetryService.Dispose();
    }
}
