using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces;
using TickerQ.Utilities.Interfaces.Managers;
using WeatherMonitor.Api.Infrastructure.Clients.Interfaces;
using WeatherMonitor.Api.Infrastructure.Persistence;
using WeatherMonitor.Domain.Deliveries;
using WeatherMonitor.Domain.Deliveries.ValueObjects;

namespace WeatherMonitor.Api.Features.MonitorProcessing;

[UsedImplicitly]
internal sealed partial class WeatherMonitorProcessor(
    ILogger<WeatherMonitorProcessor> logger,
    IBrasilApiClient api,
    AppDbContext db,
    TimeProvider time,
    ITimeTickerManager<TimeTickerEntity> manager
) : ITickerFunction
{
    public async Task ExecuteAsync(TickerFunctionContext context, CancellationToken cancellationToken = default)
    {
        await foreach (var monitor in db.Monitors.Where(monitor => monitor.Enabled == true).AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            try
            {
                var response = await api.GetForecastAsync(monitor.Location.CityCode, days: 1, cancellationToken);

                if (response is not { IsSuccessful: true, Content: not null })
                {
                    LogMonitorUnsuccessfulResponse(monitor.Id);
                    continue;
                }

                var now = time.GetUtcNow();

                var forecastDate = monitor.CalculateForecastDate(now);

                var weather = response.Content.Weather.FirstOrDefault(weather => weather.Date == forecastDate);

                if (weather is null)
                {
                    LogEmptyWeatherForMonitorLocation(monitor.Id, monitor.Location.CityCode);
                    continue;
                }

                if (!monitor.Matches(weather.Condition))
                {
                    LogWeatherConditionNotMatched(monitor.Id);
                    continue;
                }

                var scheduled = monitor.CalculateDeliverySchedule(now);

                if (await db.Deliveries.AnyAsync(delivery => delivery.ScheduledFor == scheduled &&
                                                             delivery.Payload.ClientId == monitor.ClientId &&
                                                             delivery.Payload.MonitorId == monitor.Id &&
                                                             (delivery.Status == WebhookDeliveryStatus.Pending ||
                                                              delivery.Status == WebhookDeliveryStatus.Delivered), cancellationToken))
                {
                    continue;
                }

                var delivery = new WebhookDelivery(
                    monitor.Id,
                    monitor.ClientId,
                    weather.Date,
                    scheduled,
                    monitor.Location.CityCode,
                    monitor.Location.CityName,
                    monitor.Location.State.Value,
                    weather.Condition,
                    weather.Description
                );

                await db.Deliveries.AddAsync(delivery, cancellationToken);

                await db.SaveChangesAsync(cancellationToken);

                var tickerEntity = new TimeTickerEntity
                {
                    Function = nameof(WebhookMonitorDispatcher),
                    ExecutionTime = scheduled,
                    Request = TickerHelper.CreateTickerRequest(new WebhookDeliveryEnvelope(monitor.Id, delivery.Id)),
                    Retries = 2,
                    RetryIntervals = [60, 60]
                };

                try
                {
                    var result = await manager.AddAsync(tickerEntity, cancellationToken);

                    if (result is not { IsSucceeded: true })
                    {
                        LogFailedToScheduleWebhookDelivery(monitor.Id, result.Exception);

                        throw result.Exception ?? new InvalidOperationException("Failed to schedule webhook delivery job.");
                    }

                    delivery.AssignJob(result.Result.Id.ToString());
                }
                catch
                {
                    delivery.MarkFailed();

                    throw;
                }
                finally
                {
                    await db.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Monitor {MonitorId} processing failed.", monitor.Id);
            }
        }
    }

    [LoggerMessage(LogLevel.Error, "Monitor {MonitorId} received an unsuccessful response from BrasilAPI.")]
    partial void LogMonitorUnsuccessfulResponse(Guid monitorId);

    [LoggerMessage(LogLevel.Warning, "Monitor {MonitorId} received no city with code {CityCode}.")]
    partial void LogEmptyWeatherForMonitorLocation(Guid monitorId, int cityCode);

    [LoggerMessage(LogLevel.Information, "Weather condition was not a match for monitor {MonitorId}.")]
    partial void LogWeatherConditionNotMatched(Guid monitorId);

    [LoggerMessage(LogLevel.Error, "Failed to schedule webhook delivery for monitor {MonitorId}")]
    partial void LogFailedToScheduleWebhookDelivery(Guid monitorId, Exception exception);
}