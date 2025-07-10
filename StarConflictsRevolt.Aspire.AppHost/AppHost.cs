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
    .AddDatabase("StarConflictsRevolt")
    ;

var ravenDb = builder.AddRavenDB("ravenDb", RavenDBServerSettings.Unsecured())
    .WithDataVolume("ravenDb-data")
    .WithLifetime(ContainerLifetime.Persistent)
    ;

var engine = builder.AddProject<StarConflictsRevolt_Server_GameEngine>("engine")
    .WithReference(redis)
    .WithReference(gameDb)
    .WithReference(ravenDb)
    .WaitFor(gameDb)
    .WaitFor(ravenDb)
    .WaitFor(redis)
    ;

var webapi = builder.AddProject<StarConflictsRevolt_Server_WebApi>("webapi", "http")
    .WithReference(redis)
    .WithReference(gameDb)
    .WithReference(ravenDb)
    .WaitFor(gameDb)
    .WaitFor(ravenDb)
    .WaitFor(redis)
    ;

// Add Raylib client that runs after both services are ready
var raylib = builder.AddProject<StarConflictsRevolt_Clients_Raylib>("raylib")
    .WithReference(webapi)
    .WithReference(engine)
    .WaitFor(webapi)
    .WaitFor(engine)
    ;

builder.Build().Run();