using AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgreSqlContainer();

var keycloak = builder.AddKeycloakContainer(postgres.AddKeycloakDatabase());

var redis = builder.AddRedisContainer();

var database = postgres.AddWeatherMonitorDatabase();

var dotnet = builder.AddDotNetEfDatabaseUpdateCommand(database);

var samples = builder.AddSampleApps();

builder.AddWeatherMonitorProject(keycloak, redis, database, dotnet, samples);

var app = builder.Build();

await app.RunAsync();