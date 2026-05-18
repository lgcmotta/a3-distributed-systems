using MediatR;
using Microsoft.EntityFrameworkCore;
using WeatherMonitor.Api.Infrastructure.Persistence;
using WeatherMonitor.Domain.Monitors.Exceptions;

namespace WeatherMonitor.Api.Features.GetMonitorById;

internal sealed class GetMonitorByIdCommandHandler(AppDbContext context) : IRequestHandler<GetMonitorByIdRequest, MonitorResponse>
{
    public async Task<MonitorResponse> Handle(GetMonitorByIdRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var monitor = await context.Monitors.AsNoTracking().Where(monitor =>
            monitor.Id == request.MonitorId && monitor.ClientId == request.ClientId)
            .SingleOrDefaultAsync(cancellationToken);

        if (monitor is null)
            throw new MonitorNotFoundException();

        return new MonitorResponse
        {
            MonitorId = monitor.Id,
            CityCode = monitor.Location.CityCode,
            CityName = monitor.Location.CityName,
            State = monitor.Location.State.Value,
            WeatherConditionCode = monitor.WeatherCondition.Code,
            WeatherConditionDescription = monitor.WeatherCondition.Description,
            WebhookUrl = monitor.Webhook.Url,
            Time = monitor.Webhook.ScheduleFor,
            TimeZoneId = monitor.Webhook.TimeZoneId,
            Enabled = monitor.Enabled
        };
    }
}