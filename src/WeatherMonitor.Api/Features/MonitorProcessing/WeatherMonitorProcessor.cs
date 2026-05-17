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
using WeatherMonitor.Domain.Monitors;

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
        var cityCodes = await db.SelectDistinctCityCodesAsync(cancellationToken);

        foreach (var cityCode in cityCodes)
        {
            try
            {
                var response = await api.GetForecastAsync(cityCode, days: 1, cancellationToken);

                if (response is not { IsSuccessful: true, Content: not null })
                {
                    LogCityUnsuccessfulResponse(cityCode);
                    continue;
                }

                var now = time.GetUtcNow();

                await foreach (var monitor in db.StreamMonitorsAsync(cityCode).WithCancellation(cancellationToken))
                {
                    try
                    {
                        var forecastDate = monitor.CalculateForecastDate(now);

                        var weather = response.Content.Weather.FirstOrDefault(weather => weather.Date == forecastDate);

                        if (weather is null)
                        {
                            LogForecastDateNotFound(monitor.Id, cityCode, forecastDate);
                            continue;
                        }

                        if (!monitor.Matches(weather.Condition))
                        {
                            LogWeatherConditionNotMatched(monitor.Id);
                            continue;
                        }

                        if (await db.HasDeliveryForForecastDateAsync(monitor.Id, forecastDate, cancellationToken))
                        {
                            continue;
                        }

                        var scheduled = monitor.CalculateDeliverySchedule(now);

                        var delivery = new WebhookDelivery(
                            monitor.Id,
                            monitor.ClientId,
                            forecastDate,
                            scheduled,
                            monitor.Location.CityCode,
                            monitor.Location.CityName,
                            monitor.Location.State.Value,
                            weather.Condition,
                            weather.Description
                        );

                        await db.Deliveries.AddAsync(delivery, cancellationToken);

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
            catch (Exception exception)
            {
                logger.LogError(exception, "City {CityCode} processing failed.", cityCode);
            }
        }
    }

    [LoggerMessage(LogLevel.Error, "City {CityCode} received an unsuccessful response from BrasilAPI.")]
    partial void LogCityUnsuccessfulResponse(int cityCode);

    [LoggerMessage(LogLevel.Warning, "Monitor {MonitorId} found no forecast for city {CityCode} on {ForecastDate}.")]
    partial void LogForecastDateNotFound(Guid monitorId, int cityCode, DateOnly forecastDate);

    [LoggerMessage(LogLevel.Information, "Weather condition was not a match for monitor {MonitorId}.")]
    partial void LogWeatherConditionNotMatched(Guid monitorId);

    [LoggerMessage(LogLevel.Error, "Failed to schedule webhook delivery for monitor {MonitorId}.")]
    partial void LogFailedToScheduleWebhookDelivery(Guid monitorId, Exception exception);
}

file static class WeatherMonitorProcessorExtensions
{
    extension(AppDbContext context)
    {
        internal Task<List<int>> SelectDistinctCityCodesAsync(CancellationToken cancellationToken = default)
        {
            return context.Monitors.Where(monitor => monitor.Enabled).Select(monitor => monitor.Location.CityCode).Distinct().ToListAsync(cancellationToken);
        }

        internal IAsyncEnumerable<WeatherMonitorConfiguration> StreamMonitorsAsync(int cityCode)
        {
            return context.Monitors.Where(monitor => monitor.Enabled && monitor.Location.CityCode == cityCode).AsAsyncEnumerable();
        }

        internal Task<bool> HasDeliveryForForecastDateAsync(Guid monitorId, DateOnly forecastDate, CancellationToken cancellationToken = default)
        {
            return context.Deliveries.AnyAsync(delivery => delivery.Payload.MonitorId == monitorId &&
                                                           delivery.Payload.ForecastDate == forecastDate &&
                                                           (delivery.Status == WebhookDeliveryStatus.Pending ||
                                                            delivery.Status == WebhookDeliveryStatus.Delivered), cancellationToken);
        }
    }
}