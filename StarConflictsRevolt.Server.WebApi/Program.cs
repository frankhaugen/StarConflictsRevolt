
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Server.GameEngine;

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