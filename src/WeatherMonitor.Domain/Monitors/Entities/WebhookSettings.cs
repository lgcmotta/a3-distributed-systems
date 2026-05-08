using System.Diagnostics.CodeAnalysis;

namespace WeatherMonitor.Domain.Monitors.Entities;

public sealed class WebhookSettings
{
    private WebhookSettings()
    { }

    public WebhookSettings([StringSyntax(StringSyntaxAttribute.Uri)] string url, TimeOnly scheduleFor, string? accessToken = null) : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            throw new ArgumentException("Webhook URL must be an absolute URI.", nameof(url));
        }

        Url = url;
        AccessToken = accessToken;
        ScheduleFor = scheduleFor;
    }

    public string Url { get; private set; } = string.Empty;

    public string? AccessToken { get; private set; } = string.Empty;

    public TimeOnly ScheduleFor { get; private set; }

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

    internal void Reschedule(TimeOnly time)
    {
        if (ScheduleFor == time)
        {
            return;
        }

        ScheduleFor = time;
    }
}