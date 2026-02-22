using CommunityToolkit.Aspire.Hosting.RavenDB;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Only start Redis, SQL Server, and RavenDB containers when requested (e.g. local dev).
// Set Aspire:UseContainers to false to use existing instances and pass connection strings via config/env.
var useContainersRaw = builder.Configuration["Aspire:UseContainers"];
var useContainers = useContainersRaw is null or "" || (bool.TryParse(useContainersRaw, out var uc) && uc);

var webapi = useContainers
    ? AddWebApiWithContainers(builder)
    : AddWebApiWithConnectionStringsOnly(builder);

static IResourceBuilder<ProjectResource> AddWebApiWithContainers(IDistributedApplicationBuilder b)
{
    var redis = b.AddRedis("redis")
        .WithPassword(b.AddParameter("redis-password", "My!Password123"))
        .WithDataVolume("redis-data")
        .WithLifetime(ContainerLifetime.Persistent);
    var ravenSettings = RavenDBServerSettings.Unsecured();
    ravenSettings.Port = 5090;
    var ravenDb = b.AddRavenDB("ravenDb", ravenSettings)
        .WithDataVolume("ravenDb-data")
        .WithLifetime(ContainerLifetime.Persistent);
    return b.AddProject<StarConflictsRevolt_Server_WebApi>("webapi", "https")
        .WithReference(redis)
        .WithReference(ravenDb)
        .WaitFor(ravenDb)
        .WaitFor(redis)
        .WithHttpHealthCheck(path: "/health", endpointName: "https");
}

static IResourceBuilder<ProjectResource> AddWebApiWithConnectionStringsOnly(IDistributedApplicationBuilder b)
{
    var redisConn = b.AddParameter("redis-connection", "localhost:6379");
    var ravenConn = b.AddParameter("ravendb-connection", "Url=http://localhost:8090");
    return b.AddProject<StarConflictsRevolt_Server_WebApi>("webapi", "https")
        .WithEnvironment("ConnectionStrings__redis", redisConn)
        .WithEnvironment("ConnectionStrings__ravenDb", ravenConn)
        .WithHttpHealthCheck(path: "/health", endpointName: "https");
}

var webapiHttps = webapi.GetEndpoint("https");
var blazor = builder.AddProject<StarConflictsRevolt_Clients_Blazor>("blazor", "https")
    .WithReference(webapi)
    .WaitFor(webapi)
    .WithHttpHealthCheck(path: "/health", endpointName: "https")
    .WithEnvironment("GameClientConfiguration__ApiBaseUrl", webapiHttps)
    .WithEnvironment("GameClientConfiguration__GameServerUrl", webapiHttps)
    .WithEnvironment("GameClientConfiguration__GameServerHubUrl", $"{webapiHttps}/gamehub")
    .WithEnvironment("GameClientConfiguration__CommandHubUrl", $"{webapiHttps}/commandhub")
    .WithEnvironment("GameClientConfiguration__TokenEndpoint", $"{webapiHttps}/token")
    .WithEnvironment("TokenProviderOptions__TokenEndpoint", $"{webapiHttps}/token");

builder.Build().Run();