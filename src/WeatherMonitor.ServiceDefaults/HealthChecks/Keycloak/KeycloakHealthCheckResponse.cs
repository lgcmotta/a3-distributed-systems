using JetBrains.Annotations;

namespace WeatherMonitor.ServiceDefaults.HealthChecks.Keycloak;

[UsedImplicitly]
internal record KeycloakHealthCheckResponse
{
    public required string Status { get; init; }
}