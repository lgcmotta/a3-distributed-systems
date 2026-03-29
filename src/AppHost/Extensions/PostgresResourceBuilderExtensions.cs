namespace AppHost.Extensions;

internal static class PostgresResourceBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<PostgresServerResource> AddPostgreSqlContainer()
        {
            return builder.AddPostgres(name: "PostgreSQL", port: 5432)
                .WithImageRegistry(registry: "mirror.gcr.io")
                .WithImage(image: "postgres", tag: "latest")
                .WithContainerName("postgres")
                .WithLifetime(lifetime: ContainerLifetime.Persistent)
                .WithVolume(name: "postgres_data", target: "/var/lib/postgresql");
        }
    }

    extension(IResourceBuilder<PostgresServerResource> builder)
    {
        internal IResourceBuilder<PostgresDatabaseResource> AddKeycloakDatabase()
        {
            return builder.AddDatabase(name: "KeycloakDB", databaseName: "keycloak-db");
        }

        internal IResourceBuilder<PostgresDatabaseResource> AddWeatherMonitorDatabase()
        {
            return builder.AddDatabase(name: "WeatherMonitorDB", databaseName: "weather-monitor-db");
        }
    }
}