using StarConflictsRevolt.Server.GameEngine;
using Raven.Client.Documents;
using StarConflictsRevolt.Server.Eventing;

var builder = WebApplication.CreateBuilder(args);

GameEngineStartupHelper.RegisterGameEngineServices(builder);
GameEngineStartupHelper.RegisterGameEngineDbContext(builder);
GameEngineStartupHelper.RegisterGameEngineDocumentStore(builder);

var app = builder.Build();

GameEngineStartupHelper.ConfigureGameEngine(app);

app.Run();
