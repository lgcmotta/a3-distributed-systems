namespace WeatherMonitor.Domain.Monitors.Exceptions;

public class MonitorNotFoundException(Guid monitorId)
    : Exception($"Monitor with ID {monitorId} was not found for the given client ID.");