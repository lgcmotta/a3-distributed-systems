namespace WeatherMonitor.Domain.Monitors.Exceptions;

public class CityLookupFailedException(string cityName)
    : Exception($"Unable to resolve the requested city '{cityName}' because the city lookup response did not include city data.");