using MediatR;
using System.Text.Json.Serialization;
using WeatherMonitor.Api.Contracts;

namespace WeatherMonitor.Api.Features.UpdateMonitor;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public record PatchMonitorRequest(
    [property: JsonIgnore] string ClientId,
    [property: JsonIgnore] Guid MonitorId,
    string? WebhookUrl = null,
    string? AccessToken = null,
    TimeOnly? Time = null,
    string? TimeZoneId = null,
    bool? Enabled = null
) : IRequest<MonitorResponse>;