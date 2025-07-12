using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;
using StarConflictsRevolt.Tests.TestingInfrastructure;

namespace StarConflictsRevolt.Tests.ExampleTests;

public class ServiceResolutionTest
{
    [Test]
    public void Should_throw_clear_error_when_service_missing()
    {
        var provider = new ServiceCollection()
            .AddMinimalTestServices()
            .BuildServiceProvider();

        // Try to resolve a service that is NOT registered
        Assert.Throws<InvalidOperationException>(() =>
        {
            provider.GetRequiredService<StarConflictsRevolt.Server.WebApi.Eventing.IEventStore>();
        });
    }
} 