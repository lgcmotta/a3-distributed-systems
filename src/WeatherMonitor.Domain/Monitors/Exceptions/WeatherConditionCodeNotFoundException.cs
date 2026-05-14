namespace WeatherMonitor.Domain.Monitors.Exceptions;

public class WeatherConditionCodeNotFoundException(string code)
    : Exception($"No weather conditions found for code: {code}");