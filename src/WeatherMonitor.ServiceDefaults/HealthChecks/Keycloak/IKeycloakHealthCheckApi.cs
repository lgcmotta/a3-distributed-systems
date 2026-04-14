using Refit;

namespace WeatherMonitor.ServiceDefaults.HealthChecks.Keycloak;

internal interface IKeycloakHealthCheckApi
{
    [Get("/health")]
    public Task<IApiResponse<KeycloakHealthCheckResponse>> GetHealthAsync(CancellationToken cancellationToken = default);
}