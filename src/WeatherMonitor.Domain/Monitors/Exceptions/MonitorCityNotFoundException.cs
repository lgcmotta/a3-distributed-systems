namespace WeatherMonitor.Domain.Monitors.Exceptions;

public class MonitorCityNotFoundException(string cityName, string state)
    : Exception($"No city named '{cityName}' was found in state '{state}'.");