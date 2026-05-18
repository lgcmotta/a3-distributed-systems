using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace WeatherMonitor.Api.Features.GetMonitorById;

[UsedImplicitly]
public record GetMonitorByIdRequest(
    [property: JsonIgnore] string ClientId,
    [property: FromRoute(Name = "monitorId")] Guid MonitorId
) : IRequest<MonitorResponse>;