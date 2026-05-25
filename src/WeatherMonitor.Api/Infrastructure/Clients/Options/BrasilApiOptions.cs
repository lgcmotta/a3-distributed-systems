using JetBrains.Annotations;

namespace WeatherMonitor.Api.Infrastructure.Clients.Options;

internal sealed record BrasilApiOptions
{
    public const string SectionName = "BrasilApi";

    public required Uri BaseAddress { get; init; }
    public required ResilienceOptions Resilience { get; init; }
}

[UsedImplicitly]
internal sealed record ResilienceOptions
{
    public required TimeSpan TotalRequestTimeout { get; init; } = TimeSpan.FromMinutes(1);
    public required TimeSpan AttemptTimeout { get; init; } = TimeSpan.FromMinutes(1);
    public required TimeSpan SamplingDuration { get; init; } = TimeSpan.FromMinutes(2);
}