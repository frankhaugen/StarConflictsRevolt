
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

WebApiStartupHelper.RegisterServices(builder);
WebApiStartupHelper.RegisterGameEngineDbContext(builder);
WebApiStartupHelper.RegisterRavenDb(builder);

var app = builder.Build();

WebApiStartupHelper.Configure(app);

app.Run();