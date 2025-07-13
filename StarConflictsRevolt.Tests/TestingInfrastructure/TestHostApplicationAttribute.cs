using Microsoft.Extensions.DependencyInjection;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public sealed class TestHostApplicationAttribute : DependencyInjectionDataSourceAttribute<IServiceScope>
{
    private static readonly Lazy<IServiceProvider> Root = new(() =>
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestHostApplication>();
        var provider = services.BuildServiceProvider();
        
        return provider;
    });

    public override IServiceScope CreateScope(DataGeneratorMetadata _) =>
        Root.Value.CreateAsyncScope();

    public override object? Create(IServiceScope scope, Type type)
    {
        var testHost = scope.ServiceProvider.GetRequiredService<TestHostApplication>();
        // Start the server when the test host is created
        // testHost.StartServerAsync(CancellationToken.None).GetAwaiter().GetResult();
        return testHost;
    }
} 