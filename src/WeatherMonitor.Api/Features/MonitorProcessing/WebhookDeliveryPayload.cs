namespace WeatherMonitor.Api.Features.MonitorProcessing;

internal sealed record WebhookDeliveryPayload(
    Guid DeliveryId,
    Guid MonitorId,
    string ClientId,
    DateOnly ForecastDate,
    int CityCode,
    string CityName,
    string State,
    string WeatherConditionCode,
    string WeatherConditionDescription
);