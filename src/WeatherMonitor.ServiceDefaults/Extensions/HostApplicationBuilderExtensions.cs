using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Refit;
using WeatherMonitor.ServiceDefaults.HealthChecks;
using WeatherMonitor.ServiceDefaults.HealthChecks.Keycloak;

namespace WeatherMonitor.ServiceDefaults.Extensions;

public static class HostApplicationBuilderExtensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        public TBuilder AddServiceDefaults()
        {
            builder.ConfigureOpenTelemetry();

            builder.AddDefaultHealthChecks();

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();
                http.AddServiceDiscovery();
            });

            return builder;
        }

        private TBuilder ConfigureOpenTelemetry()
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddMeter("Microsoft.Extensions.Caching.Hybrid")
                        .AddEventCountersInstrumentation(options => options
                            .AddEventSources(
                                "Microsoft.AspNetCore.Hosting",
                                "Microsoft.AspNetCore.Http.Connections",
                                "Microsoft-AspNetCore-Server-Kestrel",
                                "System.Net.Http",
                                "System.Net.NameResolution",
                                "System.Net.Security"
                            )
                        );
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName)
                        .AddAspNetCoreInstrumentation(options => options.Filter = context => context.IsNotHealthCheck())
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddSource("Microsoft.Extensions.Caching.Hybrid");
                });

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        private TBuilder AddOpenTelemetryExporters()
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }

            return builder;
        }

        private TBuilder AddDefaultHealthChecks()
        {
            builder.Services.AddHostedService<LivenessBackgroundService>();
            builder.Services.AddSingleton<LivenessHealthCheck>();

            builder.Services.AddRefitClient<IKeycloakHealthCheckApi>()
                .ConfigureHttpClient((provider, client) =>
                {
                    var configuration = provider.GetRequiredService<IConfiguration>();

                    var url = configuration.GetValue<string>("KEYCLOAK_MANAGEMENT");

                    ArgumentException.ThrowIfNullOrWhiteSpace(url);

                    client.BaseAddress = new Uri(url);
                });

            builder.Services.AddHealthChecks()
                .AddCheck<LivenessHealthCheck>("liveness", tags: ["alive"])
                .AddKeycloak(tags: ["ready"])
                .AddRedis(connectionStringFactory: provider =>
                {
                    var configuration = provider.GetRequiredService<IConfiguration>();

                    var connectionString = configuration.GetConnectionString("Redis");

                    ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

                    return connectionString;
                }, tags: ["ready"])
                .AddNpgSql(connectionStringFactory: provider =>
                {
                    var configuration = provider.GetRequiredService<IConfiguration>();

                    var connectionString = configuration.GetConnectionString(name: "WeatherMonitorDB");

                    ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

                    return connectionString;
                }, tags: ["ready"]);

            return builder;
        }
    }
}