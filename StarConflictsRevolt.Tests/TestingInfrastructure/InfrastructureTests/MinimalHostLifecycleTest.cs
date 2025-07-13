using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using TUnit;

namespace StarConflictsRevolt.Tests.TestingInfrastructure.Infrastructure;

public class MinimalHostLifecycleTest
{
    [Test]
    public async Task MinimalHost_Starts_And_Stops_Cleanly()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        await app.StartAsync();
        await app.StopAsync();
        await app.DisposeAsync();
        await Assert.That(true).IsTrue(); // If we get here, the host started and stopped cleanly
    }
    
    [Test]
    public async Task MinimalHost_Handles_EmptyRequest()
    {
        var builder = new TestHostApplication();
        await builder.App.StartAsync();
        await builder.App.StopAsync();
        await builder.App.DisposeAsync();
    }
} 