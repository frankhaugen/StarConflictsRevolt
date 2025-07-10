using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Server.WebApi.Helpers;

var builder = WebApplication.CreateBuilder(args);

GameEngineStartupHelper.RegisterGameEngineServices(builder);
GameEngineStartupHelper.RegisterGameEngineDbContext(builder);
GameEngineStartupHelper.RegisterGameEngineDocumentStore(builder);
WebApiStartupHelper.RegisterServices(builder);
WebApiStartupHelper.RegisterGameEngineDbContext(builder);
WebApiStartupHelper.RegisterRavenDb(builder);

var app = builder.Build();

GameEngineStartupHelper.ConfigureGameEngine(app);
WebApiStartupHelper.Configure(app);

app.Run();