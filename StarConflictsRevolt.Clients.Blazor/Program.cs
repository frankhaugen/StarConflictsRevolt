using StarConflictsRevolt.Clients.Blazor.Components;
using StarConflictsRevolt.Clients.Shared;
using StarConflictsRevolt.Clients.Blazor.Services;
using StarConflictsRevolt.Clients.Shared.Communication;
using StarConflictsRevolt.Clients.Shared.Http;
using StarConflictsRevolt.Clients.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add shared client services
builder.Services.Configure<GameClientConfiguration>(
    builder.Configuration.GetSection("GameClientConfiguration"));
builder.Services.AddSingleton<ISignalRService, SignalRService>();
builder.Services.AddHttpClient<IHttpApiClient, HttpApiClient>();

// Add Blazor-specific services
builder.Services.AddScoped<IGameStateService, GameStateService>();
builder.Services.AddScoped<BlazorSignalRService>();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
