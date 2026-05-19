using MediatR;
using Microsoft.EntityFrameworkCore;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.Infrastructure.Extensions;
using WeatherMonitor.Api.Infrastructure.Persistence;
using WeatherMonitor.Domain.Monitors.Exceptions;

namespace WeatherMonitor.Api.Features.GetMonitorById;

internal sealed class GetMonitorByIdCommandHandler(AppDbContext context) : IRequestHandler<GetMonitorByIdQuery, MonitorResponse>
{
    public async Task<MonitorResponse> Handle(GetMonitorByIdQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var monitor = await context.Monitors.AsNoTracking()
            .Where(monitor => monitor.Id == request.MonitorId && monitor.ClientId == request.ClientId)
            .Select(monitor => new MonitorResponse
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
                CreatedAt = EF.Property<DateTimeOffset>(monitor, "created_at").ToLocalTimeZone(monitor.Webhook.TimeZoneId),
                UpdatedAt = EF.Property<DateTimeOffset?>(monitor, "updated_at").ToLocalTimeZone(monitor.Webhook.TimeZoneId),
                Enabled = monitor.Enabled
            })
            .SingleOrDefaultAsync(cancellationToken);

        return monitor ?? throw new MonitorNotFoundException(request.MonitorId);
    }
}