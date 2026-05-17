using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.GetMonitors;

[UsedImplicitly]
internal record GetMonitorsRequest(
    [property: JsonIgnore] string ClientId,
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "size")] int Size = 10
) : IRequest<(IEnumerable<MonitorResponse>, PagedResponse)>;
