namespace AppHost.Extensions;

internal static class WeatherMonitorResourceBuilderExtensions
{
    private sealed record ConfigurationParameter([ResourceName] string Name, string ConfigurationKey)
    {
        internal string ToEnvironmentVariable() => ConfigurationKey.Replace(":", "__");
    }

    private static readonly ConfigurationParameter[] ConfigurationParameters =
    [
        new("brasil-api-base-address", "BrasilApi:BaseAddress"),
        new("brasil-api-resilience-request-timeout", "BrasilApi:Resilience:TotalRequestTimeout"),
        new("brasil-api-resilience-attempt-timeout", "BrasilApi:Resilience:AttemptTimeout"),
        new("brasil-api-resilience-sampling-duration", "BrasilApi:Resilience:SamplingDuration"),
        new("caching-handler-expiration", "CachingHandler:Expiration"),
        new("weather-monitor-db-retries", "WeatherMonitorDB:Retries"),
        new("webhook-monitor-dispatcher-timeout", "WebhookMonitorDispatcher:Timeout"),
        new("weather-monitor-processor-cron-expression", "WeatherMonitorProcessor:CronExpression"),
        new("weather-monitor-processor-max-concurrency", "WeatherMonitorProcessor:MaxConcurrency"),
        new("weather-monitor-processor-forecast-days", "WeatherMonitorProcessor:ForecastDays")
    ];

    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<ProjectResource> AddWeatherMonitorProject(
            IResourceBuilder<KeycloakResource> keycloak,
            IResourceBuilder<RedisResource> redis,
            IResourceBuilder<PostgresDatabaseResource> database,
            IResourceBuilder<ExecutableResource> dotnet,
            IReadOnlyCollection<IResourceBuilder<ContainerResource>> samples)
        {
            var weather = builder.AddProject<Projects.WeatherMonitor_Api>("WeatherMonitor")
                .WithReference(keycloak)
                .WithReference(redis)
                .WithReference(database)
                .WaitFor(keycloak)
                .WaitFor(redis)
                .WaitFor(database)
                .WaitForCompletion(dotnet)
                .WithUrlForEndpoint("http", _ => new ResourceUrlAnnotation { Url = "/scalar", DisplayText = "Scalar" })
                .WithUrlForEndpoint("http", _ => new ResourceUrlAnnotation { Url = "/tickerq/dashboard", DisplayText = "TickerQ" })
                .WithoutEmptyEndpoints()
                .WithHttpHealthCheck("/healthz/ready")
                .WithHttpHealthCheck("/healthz/live")
                .WithKeycloakEnvironment(keycloak)
                .WithReferenceToSamples(samples);

            foreach (ConfigurationParameter configuration in ConfigurationParameters)
            {
                var parameter = builder.AddParameterFromConfiguration(
                    name: configuration.Name,
                    configurationKey: configuration.ConfigurationKey,
                    secret: false
                );

                weather.WithEnvironment(configuration.ToEnvironmentVariable(), parameter);
            }

            return weather;
        }
    }

    extension(IResourceBuilder<ProjectResource> weather)
    {
        private IResourceBuilder<ProjectResource> WithoutEmptyEndpoints()
        {
            return weather.WithUrls(context => context.Urls.RemoveAll(url => url.Endpoint is { EndpointName: "http" } &&
                                                                             string.IsNullOrWhiteSpace(url.DisplayText)));
        }

        private IResourceBuilder<ProjectResource> WithReferenceToSamples(IReadOnlyCollection<IResourceBuilder<ContainerResource>> samples)
        {
            foreach (IResourceBuilder<ContainerResource> sample in samples)
            {
                weather.WithReference(sample.GetEndpoint(name: "http")).WaitFor(sample);
            }

            return weather;
        }

        private IResourceBuilder<ProjectResource> WithKeycloakEnvironment(IResourceBuilder<KeycloakResource> keycloak)
        {
            return weather.WithEnvironment(context =>
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
                context.EnvironmentVariables["KEYCLOAK_MANAGEMENT"] = keycloak.GetEndpoint("management").Url.TrimEnd('/');
            });
        }
    }
}