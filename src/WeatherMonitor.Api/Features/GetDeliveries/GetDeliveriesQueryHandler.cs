using MediatR;
using Microsoft.EntityFrameworkCore;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.Infrastructure.Extensions;
using WeatherMonitor.Api.Infrastructure.Persistence;

// ReSharper disable EntityFramework.UnsupportedServerSideFunctionCall

namespace WeatherMonitor.Api.Features.GetDeliveries;

internal sealed class GetDeliveriesQueryHandler(AppDbContext context) : IRequestHandler<GetDeliveriesRequest, (DeliveryResponse[], PagedResponse)>
{
    public async Task<(DeliveryResponse[], PagedResponse)> Handle(GetDeliveriesRequest request, CancellationToken cancellationToken = default)
    {
        var query = context.Deliveries
            .AsNoTracking()
            .Where(delivery => delivery.Payload.ClientId == request.ClientId);

        var start = request.Start?.ToDateTime(TimeOnly.MinValue);
        var end = request.End?.AddDays(1).ToDateTime(TimeOnly.MinValue);

        query = request switch
        {
            { Start: not null, End: not null } => query.Where(delivery =>
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(delivery.ScheduledFor.UtcDateTime, delivery.Payload.TimeZoneId) >= start &&
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(delivery.ScheduledFor.UtcDateTime, delivery.Payload.TimeZoneId) < end),
            { Start: not null, End: null } => query.Where(delivery =>
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(delivery.ScheduledFor.UtcDateTime, delivery.Payload.TimeZoneId) >= start),
            { Start: null, End: not null } => query.Where(delivery =>
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(delivery.ScheduledFor.UtcDateTime, delivery.Payload.TimeZoneId) < end),
            _ => query
        };

        var total = await query.CountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.Size);

        var response = await query
            .OrderByDescending(delivery => delivery.ScheduledFor)
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .Select(delivery => new DeliveryResponse
            {
                DeliveryId = delivery.Id,
                MonitorId = delivery.Payload.MonitorId,
                ForecastDate = delivery.Payload.ForecastDate,
                TimeZoneId = delivery.Payload.TimeZoneId,
                ScheduledFor = delivery.ScheduledFor.ToLocalTimeZone(delivery.Payload.TimeZoneId),
                DeliveredAt = delivery.DeliveredAt.ToLocalTimeZone(delivery.Payload.TimeZoneId),
                Status = delivery.Status.Value,
                RetryCount = delivery.RetryCount,
                FailureReason = delivery.FailureReason,
                CityCode = delivery.Payload.Location.Code,
                CityName = delivery.Payload.Location.Name,
                State = delivery.Payload.Location.State,
                WeatherConditionCode = delivery.Payload.WeatherCondition.Code,
                WeatherConditionDescription = delivery.Payload.WeatherCondition.Description,
                CreatedAt = EF.Property<DateTimeOffset>(delivery, "created_at").ToLocalTimeZone(delivery.Payload.TimeZoneId),
                UpdatedAt = EF.Property<DateTimeOffset?>(delivery, "updated_at").ToLocalTimeZone(delivery.Payload.TimeZoneId)
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