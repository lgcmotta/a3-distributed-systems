using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.GetMonitors;

[UsedImplicitly]
internal record GetMonitorsRequest(
    [property: FromQuery(Name = "page")] int Page = 1,
    [property: FromQuery(Name = "size")] int Size = 10
) : IRequest<(IEnumerable<MonitorResponse>, PagedResponse)>
{
    internal required string ClientId { get; init; } = string.Empty;
}