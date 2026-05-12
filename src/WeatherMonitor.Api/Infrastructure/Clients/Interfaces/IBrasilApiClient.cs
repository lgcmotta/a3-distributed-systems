using Refit;
using WeatherMonitor.Api.Infrastructure.Clients.Responses;

namespace WeatherMonitor.Api.Infrastructure.Clients.Interfaces;

public interface IBrasilApiClient
{
    [Get("/cptec/v1/cidade/{cityName}")]
    Task<IApiResponse<IEnumerable<BrasilApiCity>>> SearchCitiesAsync(
        string cityName,
        CancellationToken cancellationToken = default);

    [Get("/cptec/v1/clima/previsao/{cityCode}/{days}")]
    Task<IApiResponse<BrasilApiForecast>> GetForecastAsync(
        int cityCode,
        int days,
        CancellationToken cancellationToken = default);
}