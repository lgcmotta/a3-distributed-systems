using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WeatherMonitor.ServiceDefaults.HealthChecks.Keycloak;

internal class KeycloakHealthCheck(IKeycloakHealthCheckApi keycloak) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await keycloak.GetHealthAsync(cancellationToken);

            if (response is not { IsSuccessful: true, Content: not null })
            {
                return HealthCheckResult.Unhealthy();
            }

            return response.Content switch
            {
                { Status: "UP" } => HealthCheckResult.Healthy(),
                _ => HealthCheckResult.Unhealthy(),
            };
        }
        catch
        {
            return HealthCheckResult.Unhealthy();
        }
    }

    internal static KeycloakHealthCheck Factory(IServiceProvider provider)
    {
        var keycloak = provider.GetRequiredService<IKeycloakHealthCheckApi>();

        return new KeycloakHealthCheck(keycloak);
    }
}