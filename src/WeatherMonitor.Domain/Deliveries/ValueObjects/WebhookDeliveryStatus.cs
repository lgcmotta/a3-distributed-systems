using WeatherMonitor.Domain.Core;

namespace WeatherMonitor.Domain.Deliveries.ValueObjects;

public sealed record WebhookDeliveryStatus : Enumeration
{
    private WebhookDeliveryStatus(int key, string value) : base(key, value)
    {
    }

    public static WebhookDeliveryStatus Pending { get; } = new(0, nameof(Pending));
    public static WebhookDeliveryStatus Delivered { get; } = new(1, nameof(Delivered));
    public static WebhookDeliveryStatus Failed { get; } = new(2, nameof(Failed));
}