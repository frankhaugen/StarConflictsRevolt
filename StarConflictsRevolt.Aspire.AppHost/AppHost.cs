using CommunityToolkit.Aspire.Hosting.RavenDB;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
        .WithPassword(builder.AddParameter("redis-password", "My!Password123"))
        .WithDataVolume("redis-data")
        .WithLifetime(ContainerLifetime.Persistent)
    ;

var gameDb = builder.AddSqlServer("gameDb", builder.AddParameter("sqlserver-password", "My!Password123"))
        .WithDataVolume("gameDb-data")
        .WithLifetime(ContainerLifetime.Persistent)
    ;

var ravenDb = builder.AddRavenDB("ravenDb", RavenDBServerSettings.Unsecured())
        .WithDataVolume("ravenDb-data")
        .WithLifetime(ContainerLifetime.Persistent)
    ;

var webapi = builder.AddProject<StarConflictsRevolt_Server_WebApi>("webapi", "http")
        .WithReference(redis)
        .WithReference(gameDb)
        .WithReference(ravenDb)
        .WaitFor(gameDb)
        .WaitFor(ravenDb)
        .WaitFor(redis)
    ;

var blazor = builder.AddProject<StarConflictsRevolt_Clients_Blazor>("blazor", "http")
    .WithReference(webapi)
    .WaitFor(webapi)
    .WithEnvironment("GameClientConfiguration__ApiBaseUrl", webapi.GetEndpoint("http"))
    .WithEnvironment("GameClientConfiguration__GameServerHubUrl", $"{webapi.GetEndpoint("http")}/gamehub")
    .WithEnvironment("GameClientConfiguration__TokenEndpoint", $"{webapi.GetEndpoint("http")}/token");

builder.Build().Run();