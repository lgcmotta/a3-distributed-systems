using JetBrains.Annotations;
using MediatR;
using WeatherMonitor.Api.Contracts;

namespace WeatherMonitor.Api.Features.GetMonitorById;

[UsedImplicitly]
internal record GetMonitorByIdQuery(Guid MonitorId, string ClientId) : IRequest<MonitorResponse>;