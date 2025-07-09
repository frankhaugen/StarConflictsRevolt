using StarConflictsRevolt.Server.GameEngine;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.Eventing;

var builder = WebApplication.CreateBuilder(args);

// Register RavenDB DocumentStore
builder.Services.AddSingleton<IDocumentStore>(_ => new DocumentStore
{
    Urls = new[] { "http://localhost:8080" }, // TODO: Make configurable
    Database = "StarConflictsRevolt"
}.Initialize());

// Register RavenEventStore as IEventStore
builder.Services.AddSingleton<IEventStore, RavenEventStore>();

builder.Services.AddHostedService<GameUpdateService>();
builder.Services.AddHostedService<AiTurnService>();

builder.Services.AddSignalR(config =>
{
    config.EnableDetailedErrors = true;
    config.MaximumReceiveMessageSize = 1024 * 1024; // 1 MB
});

// Register event broadcasting service
builder.Services.AddHostedService<EventBroadcastService>();

var app = builder.Build();

app.MapHub<WorldHub>("/gamehub");

app.Run();
