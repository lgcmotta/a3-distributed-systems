using MediatR;
using Microsoft.EntityFrameworkCore;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.Infrastructure.Extensions;
using WeatherMonitor.Api.Infrastructure.Persistence;

namespace WeatherMonitor.Api.Features.GetMonitors;

internal sealed class GetMonitorsQueryHandler(AppDbContext context) : IRequestHandler<GetMonitorsRequest, (MonitorResponse[], PagedResponse)>
{
    public async Task<(MonitorResponse[], PagedResponse)> Handle(GetMonitorsRequest request, CancellationToken cancellationToken)
    {
        var query = context.Monitors
            .AsNoTracking()
            .Where(m => m.ClientId == request.ClientId);

        var total = await query.CountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.Size);

        var response = await query
            .OrderByDescending(monitor => EF.Property<DateTimeOffset>(monitor, "created_at"))
            .ThenByDescending(monitor => monitor.Id)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
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
            .ToArrayAsync(cancellationToken);

        var pagination = new PagedResponse
        {
            Page = request.Page,
            Size = request.Size,
            Previous = request.Page > 1 ? request.Page - 1 : 0,
            Next = request.Page < totalPages ? request.Page + 1 : 0,
            Total = total,
            TotalPages = totalPages
        };

        return (response, pagination);
    }
}