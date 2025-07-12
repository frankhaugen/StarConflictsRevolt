using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;

namespace StarConflictsRevolt.Tests.TestingInfrastructure;

public sealed class GameServerDataSourceAttribute : DependencyInjectionDataSourceAttribute<IServiceScope>
{
    private static readonly IServiceProvider Root = new ServiceCollection()
        .AddSingleton<GameServerTestHost>()
        .BuildServiceProvider();

    public override IServiceScope CreateScope(DataGeneratorMetadata _) =>
        Root.CreateAsyncScope();

    public override object? Create(IServiceScope scope, Type type) =>
        scope.ServiceProvider.GetService(type);
} 