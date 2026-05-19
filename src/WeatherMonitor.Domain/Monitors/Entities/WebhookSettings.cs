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

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            !uri.IsWellFormedOriginalString())
        {
            throw new ArgumentException("Webhook URL must be a valid absolute HTTPS/HTTP URI.", nameof(url));
        }

        Url = uri.ToString();
        AccessToken = accessToken;
        ScheduleFor = scheduleFor;
        TimeZoneId = timeZoneId;
    }

    public string Url { get; private set; } = string.Empty;

    public string? AccessToken { get; private set; }

    public TimeOnly ScheduleFor { get; private set; }

    public string TimeZoneId { get; private set; } = string.Empty;

    internal void ReconfigureTargetUrl(string? url)
    {
        if (url is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Webhook URL must not be empty or whitespace.", nameof(url));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            !uri.IsWellFormedOriginalString())
        {
            throw new ArgumentException("Webhook URL must be a valid absolute HTTPS/HTTP URI.", nameof(url));
        }

        Url = uri.ToString();
    }

    internal void ReconfigureAccessToken(string? accessToken = null)
    {
        if (accessToken is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            AccessToken = null;

            return;
        }

        AccessToken = accessToken;
    }

    internal void Reschedule(TimeOnly? time)
    {
        if (time is null || ScheduleFor == time)
        {
            return;
        }

        ScheduleFor = time.Value;
    }

    public void SwitchTimeZone(string? timeZoneId)
    {
        if (timeZoneId is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("Time zone ID cannot be empty or whitespace.");
        }

        if (TimeZoneId == timeZoneId)
        {
            return;
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _))
        {
            throw new ArgumentException($"Unknown time zone ID {timeZoneId}.", nameof(timeZoneId));
        }

        TimeZoneId = timeZoneId;
    }
}