namespace WeatherMonitor.Api.Features.MonitorProcessing;

public record WebhookDeliveryEnvelope(Guid MonitorId, Guid DeliveryId);