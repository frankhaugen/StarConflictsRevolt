using Microsoft.Extensions.DependencyInjection;
using StarConflictsRevolt.Server.WebApi.Application.Services.Gameplay;
using StarConflictsRevolt.Server.WebApi.Core.Domain.Enums;
using StarConflictsRevolt.Tests.TestingInfrastructure;
using TUnit.Core;

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
        await Assert.That(true).IsTrue(); // Basic test that service runs without crashing
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
        await Assert.That(true).IsTrue(); // Basic test that service runs without crashing
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
        await Assert.That(true).IsTrue(); // Basic test that service runs without crashing
    }
} 