file-scoped namespace TUnit.RavenDB;

using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TUnit.Core;

public sealed class RavenDbDataSourceAttribute : DependencyInjectionDataSourceAttribute<IServiceScope>
{
    private static readonly RavenTUnitDriver Driver = new();

    private static readonly IServiceProvider Root = new ServiceCollection()
        .AddSingleton<IDocumentStore>(_ => Driver.NewStore("Root"))
        .AddScoped<IAsyncDocumentSession>(sp =>
            sp.GetRequiredService<IDocumentStore>().OpenAsyncSession())
        .BuildServiceProvider();

    public override IServiceScope CreateScope(DataGeneratorMetadata _) =>
        Root.CreateAsyncScope();

    public override object? Create(IServiceScope scope, Type type) =>
        scope.ServiceProvider.GetService(type);
} 