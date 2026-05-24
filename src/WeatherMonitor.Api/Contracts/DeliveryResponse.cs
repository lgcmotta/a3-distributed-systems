namespace WeatherMonitor.Api.Contracts;

public record DeliveryResponse
{
    public required Guid DeliveryId { get; init; }
    public required Guid MonitorId { get; init; }
    public required DateOnly ForecastDate { get; init; }
    public required string TimeZoneId { get; init; }
    public required DateTimeOffset ScheduledFor { get; init; }
    public DateTimeOffset? DeliveredAt { get; init; }
    public required string Status { get; init; }
    public required int RetryCount { get; init; }
    public string? FailureReason { get; init; }
    public required int CityCode { get; init; }
    public required string CityName { get; init; }
    public required string State { get; init; }
    public required string WeatherConditionCode { get; init; }
    public required string WeatherConditionDescription { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}