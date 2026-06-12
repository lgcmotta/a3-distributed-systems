using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Interfaces;
using WeatherMonitor.Api.Infrastructure.Persistence;
using WeatherMonitor.Domain.Deliveries;
using WeatherMonitor.Domain.Monitors;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace WeatherMonitor.Api.Features.MonitorProcessing;

[UsedImplicitly]
internal sealed partial class WebhookMonitorDispatcher(
    ILogger<WebhookMonitorDispatcher> logger,
    IHttpClientFactory factory,
    AppDbContext db,
    TimeProvider time
) : ITickerFunction<WebhookDeliveryEnvelope>
{
    private static readonly ActivitySource ActivitySource = new(JsonNamingPolicy.KebabCaseLower.ConvertName(nameof(WebhookMonitorDispatcher)));

    public async Task ExecuteAsync(TickerFunctionContext<WebhookDeliveryEnvelope> context, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity(ActivityKind.Producer);

        var envelope = context.Request;

        var delivery = await db.Deliveries.FirstOrDefaultAsync(delivery => delivery.Id == envelope.DeliveryId, cancellationToken);

        if (delivery is null)
        {
            LogWebhookDeliveryNotFound(envelope.DeliveryId, context.RetryCount);

            var message = $"Webhook delivery '{envelope.DeliveryId}' was not found.";

            activity?.SetStatus(ActivityStatusCode.Error, message);

            throw new InvalidOperationException(message);
        }

        if (delivery.IsDelivered() || delivery.IsFailed())
        {
            return;
        }

        if (context is { RetryCount: > 0 })
        {
            delivery.RegisterRetry();
        }

        var monitor = await db.Monitors.FirstOrDefaultAsync(monitor => monitor.Id == envelope.MonitorId, cancellationToken);

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (monitor is null)
        {
            LogWeatherMonitorNotFound(envelope.MonitorId, context.RetryCount);

            if (context is { RetryCount: >= 2 })
            {
                delivery.MarkFailed("Weather monitor was not found during webhook dispatch.");
            }

            await db.SaveChangesAsync(cancellationToken);

            var message = $"Weather monitor '{envelope.MonitorId}' was not found.";

            activity?.SetStatus(ActivityStatusCode.Error, message);

            throw new InvalidOperationException(message);
        }

        if (monitor is { Enabled: false })
        {
            delivery.MarkFailed("Monitor was disabled before webhook dispatch.");

            await db.SaveChangesAsync(cancellationToken);

            return;
        }

        if (!delivery.Payload.MonitorId.Equals(monitor.Id) ||
            !string.Equals(delivery.Payload.ClientId, monitor.ClientId, StringComparison.Ordinal))
        {
            delivery.MarkFailed("Webhook delivery does not match the current monitor configuration.");

            await db.SaveChangesAsync(cancellationToken);

            return;
        }

        using var client = factory.CreateClient(nameof(WebhookMonitorDispatcher));

        HttpRequestMessage request = CreateWebhookHttpRequest(delivery, monitor, activity);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            delivery.MarkDelivered(time.GetUtcNow());

            activity?.SetCustomProperty("webhook.response.status_code", response.StatusCode.ToString());

        }
        catch (Exception exception)
        {
            LogWebhookDeliverySendFailed(delivery.Id, exception);

            if (context is { RetryCount: >= 2 })
            {
                delivery.MarkFailed();
            }

            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);

            throw;
        }
        finally
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private HttpRequestMessage CreateWebhookHttpRequest(WebhookDelivery delivery, WeatherMonitorConfiguration monitor, Activity? activity = null)
    {
        var content = JsonContent.Create(delivery.ToWebhookDeliveryPayload(), new MediaTypeHeaderValue(MediaTypeNames.Application.Json, charSet: "utf-8"));

        activity?.SetCustomProperty("webhook.request", content);

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(monitor.Webhook.Url),
            Method = HttpMethod.Post,
            Content = content,
            Headers =
            {
                { HeaderNames.Accept, [MediaTypeNames.Application.Json] },
                { HeaderNames.UserAgent, ["WeatherMonitor/1.0"] },
                { "X-WeatherMonitor-Delivery-Id", [delivery.Id.ToString()] },
                { "X-WeatherMonitor-Monitor-Id", [monitor.Id.ToString()] },
                { "X-WeatherMonitor-Event", ["weather.condition_matched"] },
                { "X-WeatherMonitor-Sent-At", [time.GetUtcNow().ToString("O")] },
                { "Idempotency-Key", [delivery.Id.ToString()] },
            }
        };

        if (!string.IsNullOrWhiteSpace(monitor.Webhook.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, monitor.Webhook.AccessToken);
        }

        return request;
    }

    [LoggerMessage(LogLevel.Error, "Webhook delivery '{DeliveryId}' was not found, total retries: {DeliveryRetry}")]
    partial void LogWebhookDeliveryNotFound(Guid deliveryId, int deliveryRetry);

    [LoggerMessage(LogLevel.Information, "Weather monitor '{MonitorId}' was not found, total retries: {DeliveryRetry}")]
    partial void LogWeatherMonitorNotFound(Guid monitorId, int deliveryRetry);

    [LoggerMessage(LogLevel.Error, "Delivery with '{DeliveryId}' failed to be sent")]
    partial void LogWebhookDeliverySendFailed(Guid deliveryId, Exception exception);
}

file static class WebhookMonitorDispatcherExtensions
{
    extension(WebhookDelivery delivery)
    {
        internal WebhookDeliveryPayload ToWebhookDeliveryPayload()
        {
            return new WebhookDeliveryPayload(
                delivery.Id,
                delivery.Payload.MonitorId,
                delivery.Payload.ClientId,
                delivery.Payload.ForecastDate,
                delivery.Payload.TimeZoneId,
                delivery.Payload.Location.Code,
                delivery.Payload.Location.Name,
                delivery.Payload.Location.State,
                delivery.Payload.WeatherCondition.Code,
                delivery.Payload.WeatherCondition.Description);
        }
    }
}