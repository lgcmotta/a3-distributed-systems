namespace WeatherMonitor.Api.Features.MonitorProcessing;

internal sealed record WebhookDeliveryPayload(
    Guid DeliveryId,
    Guid MonitorId,
    string ClientId,
    DateOnly ForecastDate,
    string TimeZoneId,
    int CityCode,
    string CityName,
    string State,
    string WeatherConditionCode,
    string WeatherConditionDescription
);