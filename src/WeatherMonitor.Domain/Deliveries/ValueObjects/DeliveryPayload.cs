using JetBrains.Annotations;

namespace WeatherMonitor.Domain.Deliveries.ValueObjects;

[UsedImplicitly]
public sealed record DeliveryPayload
{
    public required Guid MonitorId { get; init; }

    public required string ClientId { get; init; }

    public required DateOnly ForecastDate { get; init; }

    public required WeatherLocation Location { get; init; }

    public required WeatherCondition WeatherCondition { get; init; }
}