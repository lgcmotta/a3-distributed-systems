using WeatherMonitor.Api.Infrastructure.Clients.Interfaces;
using WeatherMonitor.Api.Infrastructure.Clients.Responses;

namespace WeatherMonitor.Api.Infrastructure.Clients;

public class BrasilApiClient(ICptecRefitApi cptecApi) : IBrasilApiClient
{
    public async Task<IReadOnlyList<CityResponseModel>> SearchCitiesAsync(string cityName, CancellationToken cancellationToken)
    {
        var response = await cptecApi.SearchCitiesAsync(cityName, cancellationToken);
        return response;
    }

    public Task<IReadOnlyList<ForecastResponseModel>> GetForecastAsync(string cityCode, int days, CancellationToken cancellationToken)
    {
        var response = cptecApi.GetForecastAsync(cityCode, days, cancellationToken);
        return response;
    }
}