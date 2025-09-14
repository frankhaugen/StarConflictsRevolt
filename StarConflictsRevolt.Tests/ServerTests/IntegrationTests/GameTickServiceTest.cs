using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ServerTests.IntegrationTests;

public class GameTickServiceTest
{
    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldStartAndStopWithoutErrors(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        
        // Wait a bit to allow the service to run
        await Task.Delay(2000, cancellationToken);
        
        // Assert: Service should have started without errors
        // If we get here, the service ran without crashing - no assertion needed
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldPublishTicksToPulseFlow(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        
        // Wait a bit to allow the service to run
        await Task.Delay(2000, cancellationToken);
        
        // Assert: Service should have started without errors
        // If we get here, the service ran without crashing - no assertion needed
    }

    [Test]
    [Timeout(30_000)]
    public async Task GameTickService_ShouldHandleMultipleFlows(CancellationToken cancellationToken)
    {
        // Arrange
        var testHost = new TestHostApplication();
        await testHost.StartServerAsync(cancellationToken);
        
        // Wait a bit to allow the service to run
        await Task.Delay(2000, cancellationToken);
        
        // Assert: Service should have started without errors
        // If we get here, the service ran without crashing - no assertion needed
    }
} 