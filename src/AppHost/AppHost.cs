using AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgreSqlContainer();

var keycloak = builder.AddKeycloakContainer(postgres.AddKeycloakDatabase());

var redis = builder.AddRedisContainer();

builder.AddWeatherMonitorProject(keycloak, redis, postgres.AddWeatherMonitorDatabase());

var app = builder.Build();

await app.RunAsync();