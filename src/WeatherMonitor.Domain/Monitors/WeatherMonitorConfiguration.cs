using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Monitors.Entities;
using WeatherMonitor.Domain.Monitors.ValueObjects;

namespace WeatherMonitor.Domain.Monitors;

public sealed class WeatherMonitorConfiguration : IAggregateRoot
{
    private WeatherMonitorConfiguration()
    {
    }

    public WeatherMonitorConfiguration(
        string clientId,
        int cityCode,
        string cityName,
        string state,
        string weatherConditionCode,
        string webhookUrl,
        TimeOnly time,
        string timeZoneId,
        string? accessToken = null) : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);

        if (!WeatherCondition.TryFromCode(weatherConditionCode, out var weatherCondition) || weatherCondition is null)
        {
            throw new ArgumentException("Unknown weather condition code", nameof(weatherConditionCode));
        }

        ClientId = clientId.Trim();
        Location = MonitorLocation.Create(cityCode, cityName, state);
        WeatherCondition = weatherCondition;
        Webhook = new WebhookSettings(webhookUrl, time, timeZoneId, accessToken);
        Enabled = true;
    }

    public Guid Id { get; private set; }

    public string ClientId { get; private set; } = null!;

    public MonitorLocation Location { get; private set; } = null!;

    public WeatherCondition WeatherCondition { get; private set; } = null!;

    public WebhookSettings Webhook { get; private set; } = null!;

    public bool Enabled { get; private set; }

    public void ReconfigureWebhookTarget(string url, string? accessToken)
    {
        Webhook.Reconfigure(url, accessToken);
    }

    public void Reschedule(TimeOnly time)
    {
        Webhook.Reschedule(time);
    }

    public void Enable()
    {
        if (Enabled)
        {
            return;
        }

        Enabled = true;
    }

    public void Disable()
    {
        if (!Enabled)
        {
            return;
        }

        Enabled = false;
    }

    public bool Matches(string condition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(condition);

        return WeatherCondition.TryFromCode(condition, out var weatherCondition)
               && weatherCondition is not null
               && WeatherCondition.Code == weatherCondition.Code;
    }

    public DateTime CalculateDeliverySchedule(DateTimeOffset now)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(Webhook.TimeZoneId);

        var local = TimeZoneInfo.ConvertTime(now, timeZone);

        var scheduled = DateOnly.FromDateTime(local.DateTime).ToDateTime(Webhook.ScheduleFor, DateTimeKind.Unspecified);

        if (scheduled <= local.DateTime)
        {
            scheduled = scheduled.AddDays(1);
        }

        return TimeZoneInfo.ConvertTimeToUtc(scheduled, timeZone);
    }
}