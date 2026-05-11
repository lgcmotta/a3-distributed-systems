using System.Diagnostics.CodeAnalysis;

namespace WeatherMonitor.Domain.Monitors.Entities;

public sealed class WebhookSettings
{
    private WebhookSettings()
    { }

    public WebhookSettings([StringSyntax(StringSyntaxAttribute.Uri)] string url, TimeOnly scheduleFor, string timeZoneId, string? accessToken = null) : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _))
        {
            throw new ArgumentException($"'{timeZoneId}' is not a valid time zone ID. Use an IANA time zone ID.", nameof(timeZoneId));
        }

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new ArgumentException("Webhook URL must be an absolute URI.", nameof(url));
        }

        Url = url.Trim();
        AccessToken = accessToken;
        ScheduleFor = scheduleFor;
        TimeZoneId = timeZoneId;
    }

    public string Url { get; private set; } = string.Empty;

    public string? AccessToken { get; private set; }

    public TimeOnly ScheduleFor { get; private set; }

    public string TimeZoneId { get; private set; } = string.Empty;

    internal void Reconfigure(string url, string? accessToken = null)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new ArgumentException("Webhook URL must be an absolute URI.", nameof(url));
        }

        Url = url;

        if (accessToken is not null)
        {
            AccessToken = accessToken;
        }
    }

    internal void Reschedule(TimeOnly time, string? timeZoneId = null)
    {
        if (ScheduleFor == time)
        {
            return;
        }

        ScheduleFor = time;

        if (timeZoneId is not null &&
            TimeZoneId != timeZoneId &&
            TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _))
        {
            TimeZoneId = timeZoneId;
        }
    }
}