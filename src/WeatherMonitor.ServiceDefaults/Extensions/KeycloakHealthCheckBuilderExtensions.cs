using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WeatherMonitor.ServiceDefaults.HealthChecks.Keycloak;

namespace WeatherMonitor.ServiceDefaults.Extensions;

internal static class KeycloakHealthCheckBuilderExtensions
{
    extension(IHealthChecksBuilder builder)
    {
        internal IHealthChecksBuilder AddKeycloak(IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        {
            return builder.Add(
                new HealthCheckRegistration(
                    name: "Keycloak",
                    factory: KeycloakHealthCheck.Factory,
                    failureStatus: HealthStatus.Unhealthy,
                    tags,
                    timeout
                )
            );
        }
    }
}