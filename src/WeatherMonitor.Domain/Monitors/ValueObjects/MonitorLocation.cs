using WeatherMonitor.Domain.Core;

namespace WeatherMonitor.Domain.Monitors.ValueObjects;

public sealed record MonitorLocation
{
    private MonitorLocation()
    {

    }

    internal static MonitorLocation Create(int cityCode, string cityName, string state)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cityCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);

        if (state is not { Length: 2 })
        {
            throw new ArgumentException("State (UF) must have exactly 2 characters.", nameof(state));
        }

        if (!Enumeration.TryParseByValue<BrazilianState>(state.Trim().ToUpperInvariant(), out var brazilianState))
        {
            throw new ArgumentException("State (UF) must be a valid Brazilian state.", nameof(state));
        }

        return new MonitorLocation
        {
            CityCode = cityCode,
            CityName = cityName.Trim(),
            State = brazilianState
        };
    }

    public required int CityCode { get; init; }

    public required string CityName { get; init; }

    public required BrazilianState State { get; init; }
}