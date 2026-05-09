using JetBrains.Annotations;

namespace WeatherMonitor.Domain.Deliveries.ValueObjects;

[UsedImplicitly]
public sealed record WeatherCondition
{
    public required string Code { get; init; }

    public required string Description { get; init; }
}