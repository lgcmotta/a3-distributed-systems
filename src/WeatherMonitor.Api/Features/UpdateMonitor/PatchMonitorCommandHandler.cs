using MediatR;
using Microsoft.EntityFrameworkCore;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.Infrastructure.Extensions;
using WeatherMonitor.Api.Infrastructure.Persistence;
using WeatherMonitor.Api.Infrastructure.Persistence.Extensions;
using WeatherMonitor.Domain.Monitors;
using WeatherMonitor.Domain.Monitors.Exceptions;

// ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage

// ReSharper disable EntityFramework.NPlusOne.IncompleteDataQuery

namespace WeatherMonitor.Api.Features.UpdateMonitor;

internal sealed partial class PatchMonitorCommandHandler(
    ILogger<PatchMonitorCommandHandler> logger,
    AppDbContext db
) : IRequestHandler<PatchMonitorRequest, MonitorResponse>
{
    public async Task<MonitorResponse> Handle(PatchMonitorRequest request, CancellationToken cancellationToken = default)
    {
        var monitor = await db.Monitors
            .FirstOrDefaultAsync(monitor => monitor.Id == request.MonitorId &&
                                            monitor.ClientId == request.ClientId, cancellationToken);

        if (monitor is null)
        {
            LogMonitorNotFoundForGivenClient(request.MonitorId, request.ClientId);

            throw new MonitorNotFoundException(request.MonitorId);
        }

        var targetTime = request.Time ?? monitor.Webhook.ScheduleFor;
        var targetTimeZoneId = request.TimeZoneId ?? monitor.Webhook.TimeZoneId;

        if (monitor.HasScheduleIdentityChange(targetTime, targetTimeZoneId) &&
            await db.HasDuplicateMonitorAsync(monitor, targetTime, targetTimeZoneId, cancellationToken))
        {
            throw new DuplicateWeatherMonitorException(
                monitor.Location.CityName,
                monitor.Location.State.Value,
                targetTime,
                targetTimeZoneId
            );
        }

        monitor.ReconfigureWebhookTargetUrl(request.WebhookUrl);
        monitor.ReconfigureWebhookAccessToken(request.AccessToken);
        monitor.Reschedule(request.Time);
        monitor.SwitchTimeZone(request.TimeZoneId);

        Action<WeatherMonitorConfiguration> changeMonitorState = request switch
        {
            { Enabled: true } => m => m.Enable(),
            { Enabled: false } => m => m.Disable(),
            { Enabled: null } => _ => { }
        };

        changeMonitorState(monitor);

        await db.SaveChangesAsync(cancellationToken);

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
            CreatedAt = db.ReadCreatedAtShadowProperty(monitor).ToLocalTimeZone(monitor.Webhook.TimeZoneId),
            UpdatedAt = db.ReadUpdatedAtShadowProperty(monitor).ToLocalTimeZone(monitor.Webhook.TimeZoneId),
            Enabled = monitor.Enabled
        };
    }

    [LoggerMessage(level: LogLevel.Error, message: "Monitor with id {MonitorId} was not found for client {ClientId}")]
    partial void LogMonitorNotFoundForGivenClient(Guid monitorId, string clientId);
}

file static class PatchMonitorCommandHandlerExtensions
{
    extension(AppDbContext context)
    {
        internal Task<bool> HasDuplicateMonitorAsync(WeatherMonitorConfiguration monitor, TimeOnly targetTime, string targetTimeZoneId,
            CancellationToken cancellationToken = default)
        {
            return context.Monitors.AnyAsync(wmc => wmc.Id != monitor.Id &&
                                                    wmc.ClientId == monitor.ClientId &&
                                                    wmc.Location.CityCode == monitor.Location.CityCode &&
                                                    wmc.Webhook.ScheduleFor == targetTime &&
                                                    wmc.Webhook.TimeZoneId == targetTimeZoneId, cancellationToken);
        }
    }
}