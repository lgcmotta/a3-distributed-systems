namespace AppHost.Extensions;

internal static class WeatherMonitorResourceBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<ProjectResource> AddWeatherMonitorProject(
            IResourceBuilder<KeycloakResource> keycloak,
            IResourceBuilder<RedisResource> redis,
            IResourceBuilder<PostgresDatabaseResource> database)
        {
            var weather = builder.AddProject<Projects.WeatherMonitor_Api>("WeatherMonitor")
                .WithReference(keycloak)
                .WithReference(redis)
                .WithReference(database)
                .WaitFor(keycloak)
                .WaitFor(redis)
                .WaitFor(database)
                .WithUrl("/scalar", displayText: "Scalar")
                .WithHttpHealthCheck("/healthz/ready")
                .WithHttpHealthCheck("/healthz/live")
                .WithEnvironment(context =>
                {
                    var baseUrl = keycloak.GetEndpoint("http").Url.TrimEnd('/');

                    context.EnvironmentVariables["Keycloak__Realm"] = "weather-monitor";
                    context.EnvironmentVariables["Keycloak__AuthServerUrl"] = baseUrl;
                    context.EnvironmentVariables["Keycloak__AdminUrl"] = $"{baseUrl}/api/v1";
                    context.EnvironmentVariables["Keycloak__SslRequired"] = "external";
                    context.EnvironmentVariables["Keycloak__Resource"] = "weather_api";
                    context.EnvironmentVariables["Keycloak__PublicClient"] = "false";
                    context.EnvironmentVariables["Keycloak__ConfidentialPort"] = "0";
                    context.EnvironmentVariables["Keycloak__VerifyTokenAudience"] = "false";
                    context.EnvironmentVariables["Keycloak__RequireHttpsMetadata"] = "false";
                });

            return weather;
        }
    }
}