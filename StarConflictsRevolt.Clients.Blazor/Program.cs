using StarConflictsRevolt.Aspire.ServiceDefaults;
using StarConflictsRevolt.Clients.Blazor.Components;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Clients.Blazor.Services;
using StarConflictsRevolt.Clients.Shared.Communication;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Shared.Authentication;
using StarConflictsRevolt.Clients.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddServiceDefaults(); // OpenTelemetry, health checks, service discovery, resilience

// Add shared client services
builder.Services.Configure<GameClientConfiguration>(
    builder.Configuration.GetSection("GameClientConfiguration"));
builder.Services.AddSingleton<ISignalRService, SignalRService>();
builder.Services.AddHostedService<SignalRStartupHostedService>();

// Add authentication and HTTP client services
builder.Services.AddStarConflictsHttpClients(builder.Configuration, "GameApi", client =>
{
    var apiBaseUrl = builder.Configuration["GameClientConfiguration:ApiBaseUrl"];
    if (!string.IsNullOrEmpty(apiBaseUrl))
    {
        client.BaseAddress = new Uri(apiBaseUrl);
    }
});

// Add Blazor-specific services
builder.Services.AddScoped<IClientIdProvider, ClientIdProvider>();
builder.Services.AddScoped<IClientSessionStorage, ClientSessionStorage>();
builder.Services.AddScoped<IGameStateService, GameStateService>();
builder.Services.AddScoped<BlazorSignalRService>();
builder.Services.AddSingleton<TelemetryService>();
builder.Services.AddScoped<IJavaScriptInteropService, JavaScriptInteropService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapDefaultEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
