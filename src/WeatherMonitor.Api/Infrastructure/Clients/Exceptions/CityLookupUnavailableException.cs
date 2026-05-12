namespace WeatherMonitor.Api.Infrastructure.Clients.Exceptions;

public class CityLookupUnavailableException(string city)
    : Exception($"Unable to resolve the requested city ('{city}') because the city lookup service returned an unsuccessful response");