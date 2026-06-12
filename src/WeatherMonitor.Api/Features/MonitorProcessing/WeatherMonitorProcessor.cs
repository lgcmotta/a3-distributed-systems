using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
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
    IOptionsSnapshot<ProcessorOptions> options,
    IBrasilApiClient api,
    AppDbContext db,
    TimeProvider time,
    ITimeTickerManager<TimeTickerEntity> manager
) : ITickerFunction
{
    private static readonly ActivitySource ActivitySource = new(JsonNamingPolicy.KebabCaseLower.ConvertName(nameof(WeatherMonitorProcessor)));

    public async Task ExecuteAsync(TickerFunctionContext context, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Server);

        var cityCodes = await db.SelectDistinctCityCodesAsync(cancellationToken);

        foreach (var cityCode in cityCodes)
        {
            try
            {
                var response = await api.GetForecastAsync(cityCode, days: options.Value.ForecastDays, cancellationToken);

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
                            monitor.Webhook.TimeZoneId,
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

                        Guid? jobId = null;

                        try
                        {
                            var ticker = await manager.AddAsync(tickerEntity, cancellationToken);

                            if (ticker is not { IsSucceeded: true })
                            {
                                LogFailedToScheduleWebhookDelivery(monitor.Id, ticker.Exception);

                                throw ticker.Exception ?? new InvalidOperationException("Failed to schedule webhook delivery job.");
                            }

                            jobId = ticker.Result.Id;

                            delivery.AssignJob(jobId.Value.ToString());
                        }
                        catch (Exception exception)
                        {
                            delivery.MarkFailed();

                            if (jobId is not null)
                            {
                                await manager.DeleteAsync(jobId.Value, cancellationToken);
                            }

                            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                            activity?.SetCustomProperty("error", exception);

                            throw;
                        }
                        finally
                        {
                            await db.SaveChangesAsync(cancellationToken);
                        }
                    }
                    catch (Exception exception)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                        activity?.SetCustomProperty("error", exception);

                        logger.LogError(exception, "Monitor {MonitorId} processing failed.", monitor.Id);
                    }
                }
            }
            catch (Exception exception)
            {
                activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
                activity?.SetCustomProperty("error", exception);

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