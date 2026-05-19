namespace WeatherMonitor.Api.Contracts;

public record MonitorResponse
{
    public required Guid MonitorId { get; init; }
    public required int CityCode { get; init; }
    public required string CityName { get; init; }
    public required string State { get; init; }
    public required string WeatherConditionCode { get; init; }
    public required string WeatherConditionDescription { get; init; }
    public required string WebhookUrl { get; init; }
    public required TimeOnly Time { get; init; }
    public required string TimeZoneId { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset? UpdatedAt { get; init; }
    public required bool Enabled { get; init; }
}