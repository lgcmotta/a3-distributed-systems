using MediatR;
using System.Text.Json.Serialization;
using WeatherMonitor.Api.Contracts;

namespace WeatherMonitor.Api.Features.CreateMonitor;

public sealed record CreateMonitorRequest(
    [property: JsonIgnore] string ClientId,
    string City,
    string State,
    string WeatherConditionCode,
    TimeOnly Time,
    string TimeZoneId,
    string WebhookUrl,
    string? AccessToken = null
) : IRequest<MonitorResponse>;