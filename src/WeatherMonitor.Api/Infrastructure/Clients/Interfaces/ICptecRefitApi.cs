using Refit;
using WeatherMonitor.Api.Infrastructure.Clients.Responses;

namespace WeatherMonitor.Api.Infrastructure.Clients.Interfaces;

public interface ICptecRefitApi
{
    [Get("/cptec/v1/cidade/{cityName}")]
    Task<IReadOnlyList<CityResponseModel>> SearchCitiesAsync(
        string cityName,
        CancellationToken cancellationToken);

    [Get("/cptec/v1/clima/previsao/{cityCode}/{days}")]
    Task<IReadOnlyList<ForecastResponseModel>> GetForecastAsync(
        string cityCode,
        int days,
        CancellationToken cancellationToken);
}