using StarConflictsRevolt.Server.GameEngine;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.Eventing;
using StarConflictsRevolt.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add ServiceDefaults for service discovery and observability
builder.AddServiceDefaults();

GameEngineStartupHelper.RegisterGameEngineServices(builder);
GameEngineStartupHelper.RegisterGameEngineDbContext(builder);
GameEngineStartupHelper.RegisterGameEngineDocumentStore(builder);

var app = builder.Build();

GameEngineStartupHelper.ConfigureGameEngine(app);

app.Run();
