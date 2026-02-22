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
    var gameDb = b.AddSqlServer("gameDb", b.AddParameter("sqlserver-password", "My!Password123"))
        .WithDataVolume("gameDb-data")
        .WithLifetime(ContainerLifetime.Persistent);
    var ravenDb = b.AddRavenDB("ravenDb", RavenDBServerSettings.Unsecured())
        .WithDataVolume("ravenDb-data")
        .WithLifetime(ContainerLifetime.Persistent);
    return b.AddProject<StarConflictsRevolt_Server_WebApi>("webapi", "http")
        .WithReference(redis)
        .WithReference(gameDb)
        .WithReference(ravenDb)
        .WaitFor(gameDb)
        .WaitFor(ravenDb)
        .WaitFor(redis)
        .WithHttpHealthCheck(path: "/health", endpointName: "http");
}

static IResourceBuilder<ProjectResource> AddWebApiWithConnectionStringsOnly(IDistributedApplicationBuilder b)
{
    var redisConn = b.AddParameter("redis-connection", "localhost:6379");
    var gameDbConn = b.AddParameter("gamedb-connection", "Server=localhost;Database=gameDb;User Id=sa;Password=My!Password123;TrustServerCertificate=True");
    var ravenConn = b.AddParameter("ravendb-connection", "Url=http://localhost:8080");
    return b.AddProject<StarConflictsRevolt_Server_WebApi>("webapi", "http")
        .WithEnvironment("ConnectionStrings__redis", redisConn)
        .WithEnvironment("ConnectionStrings__gameDb", gameDbConn)
        .WithEnvironment("ConnectionStrings__ravenDb", ravenConn)
        .WithHttpHealthCheck(path: "/health", endpointName: "http");
}

var webapiHttp = webapi.GetEndpoint("http");
var blazor = builder.AddProject<StarConflictsRevolt_Clients_Blazor>("blazor", "http")
    .WithReference(webapi)
    .WaitFor(webapi)
    .WithHttpHealthCheck(path: "/health", endpointName: "http")
    .WithEnvironment("GameClientConfiguration__ApiBaseUrl", webapiHttp)
    .WithEnvironment("GameClientConfiguration__GameServerUrl", webapiHttp)
    .WithEnvironment("GameClientConfiguration__GameServerHubUrl", $"{webapiHttp}/gamehub")
    .WithEnvironment("GameClientConfiguration__CommandHubUrl", $"{webapiHttp}/commandhub")
    .WithEnvironment("GameClientConfiguration__TokenEndpoint", $"{webapiHttp}/token")
    .WithEnvironment("TokenProviderOptions__TokenEndpoint", $"{webapiHttp}/token");

builder.Build().Run();