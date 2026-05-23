using MediatR;
using Microsoft.EntityFrameworkCore;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.Infrastructure.Extensions;
using WeatherMonitor.Api.Infrastructure.Persistence;

namespace WeatherMonitor.Api.Features.GetDeliveries;

internal sealed class GetDeliveriesQueryHandler(AppDbContext context)
    : IRequestHandler<GetDeliveriesRequest, (DeliveryResponse[], PagedResponse)>
{
    public async Task<(DeliveryResponse[], PagedResponse)> Handle(
        GetDeliveriesRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var query = context.Deliveries
            .AsNoTracking()
            .Where(delivery => delivery.Payload.ClientId == request.ClientId);

        if (request.Start.HasValue)
        {
            var startUtc = new DateTimeOffset(
                request.Start.Value.ToDateTime(TimeOnly.MinValue),
                TimeSpan.Zero);

            query = query.Where(delivery => delivery.ScheduledFor >= startUtc);
        }

        if (request.End.HasValue)
        {
            var endUtc = new DateTimeOffset(
                request.End.Value.ToDateTime(TimeOnly.MaxValue),
                TimeSpan.Zero);

            query = query.Where(delivery => delivery.ScheduledFor <= endUtc);
        }

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
                ScheduledFor = delivery.ScheduledFor.ToLocalTimeZone(delivery.Payload.TimeZoneId),
                Status = delivery.Status.Value,
                RetryCount = delivery.RetryCount,
                CityCode = delivery.Payload.Location.Code,
                CityName = delivery.Payload.Location.Name,
                State = delivery.Payload.Location.State,
                WeatherConditionCode = delivery.Payload.WeatherCondition.Code,
                WeatherConditionDescription = delivery.Payload.WeatherCondition.Description
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
