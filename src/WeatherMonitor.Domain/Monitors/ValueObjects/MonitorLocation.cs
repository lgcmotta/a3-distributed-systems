namespace WeatherMonitor.Domain.Monitors.ValueObjects;

public sealed record MonitorLocation
{
    private MonitorLocation()
    {

    }

    private MonitorLocation(string cityCode, string cityName, string state) : this()
    {
        CityCode = cityCode;
        CityName = cityName;
        State = state;
    }

    internal static MonitorLocation Create(string cityCode, string cityName, string state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cityCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);

        if (state is not { Length: 2 })
        {
            throw new ArgumentException("State (UF) must have exactly 2 characters.", nameof(state));
        }

        return new MonitorLocation
        {
            CityCode = cityCode.Trim(),
            CityName = cityName.Trim(),
            State = state.Trim().ToUpperInvariant()
        };
    }

    public required string CityCode { get; init; }

    public required string CityName { get; init; }

    public required string State { get; init; }
}