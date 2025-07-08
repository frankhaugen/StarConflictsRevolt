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

var engine = builder.AddProject<StarConflictsRevolt_Server_GameEngine>("engine")
    .WithReference(redis)
    .WithReference(gameDb)
    .WithReference(ravenDb)
    .WaitFor(gameDb)
    .WaitFor(ravenDb)
    .WaitFor(redis)
    ;

var webapi = builder.AddProject<StarConflictsRevolt_Server_WebApi>("webapi")
    .WithReference(redis)
    .WithReference(gameDb)
    .WithReference(ravenDb)
    .WaitFor(gameDb)
    .WaitFor(ravenDb)
    .WaitFor(redis)
    .WaitFor(engine)
    ;

// var webappserver = builder.AddProject<StarConflictsRevolt_Server_WebApp>("webappserver")
//     .WithReference(redis)
//     .WithReference(gameDb)
//     .WithReference(ravenDb)
//     .WithReference(webapi)
//     .WithReference(engine)
//     .WaitFor(engine)
//     .WaitFor(webapi)
//     ;

// var webapp = builder.AddProject<StarConflictsRevolt_Clients_WebApp>("webapp")
//     .WithReference(redis)
//     .WithReference(gameDb)
//     .WithReference(ravenDb)
//     .WithReference(webapi)
//     .WithReference(webappserver)
//     .WaitFor(engine)
//     .WaitFor(webapi)
//     .WaitFor(webappserver)
//     ;

// REMOVED: Raylib client from Aspire orchestrator
// var raylib = builder.AddProject<StarConflictsRevolt_Clients_Raylib>("raylib")
//     .WithReference(webapi)
//     .WithReference(webappserver)
//     .WaitFor(engine)
//     .WaitFor(webapi)
//     .WithEnvironment("GameClientConfiguration__GameServerHubUrl", $"{engine.GetEndpoint("http")}/gamehub");

builder.Build().Run();