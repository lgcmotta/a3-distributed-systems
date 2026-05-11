using AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgreSqlContainer();

var keycloak = builder.AddKeycloakContainer(postgres.AddKeycloakDatabase());

var redis = builder.AddRedisContainer();

var database = postgres.AddWeatherMonitorDatabase();

var dotnet = builder.AddDotNetEfDatabaseUpdateCommand(database);

builder.AddWeatherMonitorProject(keycloak, redis, database, dotnet);

var app = builder.Build();

await app.RunAsync();