using JetBrains.Annotations;

namespace WeatherMonitor.Domain.Deliveries.ValueObjects;

[UsedImplicitly]
public sealed record WeatherLocation
{
    public required int Code { get; init; }

    public required string Name { get; init; }

    public required string State { get; init; }
}