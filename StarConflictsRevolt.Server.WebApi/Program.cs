
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StarConflictsRevolt.Server.WebApi;
using StarConflictsRevolt.Aspire.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add ServiceDefaults first for proper service discovery and observability
builder.AddServiceDefaults();

// Add JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = JwtConfig.Issuer,
            ValidAudience = JwtConfig.Audience,
            IssuerSigningKey = JwtConfig.GetSymmetricSecurityKey(),
        };
    });

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

WebApiStartupHelper.RegisterServices(builder);
WebApiStartupHelper.RegisterGameEngineDbContext(builder);
WebApiStartupHelper.RegisterRavenDb(builder);

var app = builder.Build();

// Enable authentication/authorization middleware
app.UseAuthentication();
app.UseAuthorization();

WebApiStartupHelper.Configure(app);

app.Run();