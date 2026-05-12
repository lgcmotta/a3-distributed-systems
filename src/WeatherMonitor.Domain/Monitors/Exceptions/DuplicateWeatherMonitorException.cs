namespace WeatherMonitor.Domain.Monitors.Exceptions;

public class DuplicateWeatherMonitorException(string city, string state, TimeOnly time, string timeZoneId)
    : Exception($"A weather monitor already exists for city '{city}' ({state}) at {time:HH:mm:ss} in time zone '{timeZoneId}'.");