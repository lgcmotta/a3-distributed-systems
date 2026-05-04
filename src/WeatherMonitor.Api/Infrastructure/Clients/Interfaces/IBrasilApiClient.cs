using WeatherMonitor.Api.Infrastructure.Clients.Responses;

namespace WeatherMonitor.Api.Infrastructure.Clients.Interfaces;

public interface IBrasilApiClient
{
    Task<IReadOnlyList<CityResponseModel>> SearchCitiesAsync(
        string cityName,
        CancellationToken cancellationToken);
    
    Task<IReadOnlyList<ForecastResponseModel>> GetForecastAsync(
        string cityCode,
        int days,
        CancellationToken cancellationToken);
}