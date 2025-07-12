using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public sealed class TestHostApplicationAttribute : DependencyInjectionDataSourceAttribute<IServiceScope>
{
    private static readonly Lazy<IServiceProvider> Root = new(() =>
        new ServiceCollection()
            .AddSingleton<TestHostApplication>()
            .BuildServiceProvider());

    public override IServiceScope CreateScope(DataGeneratorMetadata _) =>
        Root.Value.CreateAsyncScope();

    public override object? Create(IServiceScope scope, Type type) =>
        scope.ServiceProvider.GetService(type);
} 