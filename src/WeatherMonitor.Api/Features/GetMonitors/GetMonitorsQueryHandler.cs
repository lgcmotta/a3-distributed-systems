using MediatR;
using Microsoft.EntityFrameworkCore;
using WeatherMonitor.Api.Infrastructure.Persistence;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.GetMonitors;

internal sealed class GetMonitorsQueryHandler(AppDbContext context) : IRequestHandler<GetMonitorsRequest, (IEnumerable<MonitorResponse>, PagedResponse)>
{
    public async Task<(IEnumerable<MonitorResponse>, PagedResponse)> Handle(GetMonitorsRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = context.Monitors
            .AsNoTracking()
            .Where(m => m.ClientId == request.ClientId);

        var total = await query.CountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.Size);

        var response = await query
            .OrderByDescending(monitor => EF.Property<DateTimeOffset>(monitor, "created_at"))
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
                Enabled = monitor.Enabled
            })
            .ToListAsync(cancellationToken);

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