using System.Diagnostics.CodeAnalysis;
using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Deliveries.ValueObjects;

namespace WeatherMonitor.Domain.Deliveries;

public sealed class WebhookDelivery : IAggregateRoot
{
    private WebhookDelivery()
    {
    }

    [SetsRequiredMembers]
    public WebhookDelivery(
        Guid monitorId,
        string clientId,
        DateOnly forecastDate,
        DateTimeOffset scheduledFor,
        int cityCode,
        string cityName,
        string state,
        string weatherConditionCode,
        string weatherConditionDescription) : this()
    {
        if (monitorId == Guid.Empty)
        {
            throw new ArgumentException("Monitor ID should be a valid Guid", nameof(monitorId));
        }

        if (forecastDate == default)
        {
            throw new ArgumentException("Forecast Date should not be a default", nameof(forecastDate));
        }

        if (scheduledFor == default)
        {
            throw new ArgumentException("Scheduled date should not be a default", nameof(scheduledFor));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cityCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityName);
        ArgumentException.ThrowIfNullOrWhiteSpace(state);
        ArgumentException.ThrowIfNullOrWhiteSpace(weatherConditionCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(weatherConditionDescription);

        ScheduledFor = scheduledFor;
        Status = WebhookDeliveryStatus.Pending;
        Payload = new DeliveryPayload
        {
            MonitorId = monitorId,
            ClientId = clientId.Trim(),
            ForecastDate = forecastDate,
            Location = new WeatherLocation { Code = cityCode, Name = cityName.Trim(), State = state.Trim().ToUpperInvariant() },
            WeatherCondition = new WeatherCondition { Code = weatherConditionCode.Trim(), Description = weatherConditionDescription.Trim() }
        };
    }

    public Guid Id { get; private set; }

    public DateTimeOffset ScheduledFor { get; private set; }

    public DateTimeOffset? DeliveredAt { get; private set; }

    public required DeliveryPayload Payload { get; init; }

    public WebhookDeliveryStatus Status { get; private set; } = WebhookDeliveryStatus.Pending;

    public int RetryCount { get; private set; }

    public string? JobId { get; private set; }

    public string? FailureReason { get; private set; }

    public void AssignJob(string jobId)
    {
        if (Status.IsDelivered())
        {
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(jobId);

        var normalizedJobId = jobId.Trim();

        if (string.Equals(normalizedJobId, JobId, StringComparison.Ordinal))
        {
            return;
        }

        if (JobId is not null)
        {
            throw new InvalidOperationException("A different job is already assigned to this delivery.");
        }

        JobId = normalizedJobId;
    }

    public void RegisterRetry()
    {
        if (!Status.IsPending())
        {
            throw new InvalidOperationException("A terminal delivery cannot be changed.");
        }

        RetryCount++;
    }

    public void MarkDelivered(DateTimeOffset deliveredAt)
    {
        if (deliveredAt == default)
        {
            throw new ArgumentException("Delivered date should not be a default", nameof(deliveredAt));
        }

        if (Status.IsDelivered())
        {
            return;
        }

        if (Status.IsFailed())
        {
            throw new InvalidOperationException("A failed delivery cannot be marked as delivered.");
        }

        Status = WebhookDeliveryStatus.Delivered;
        DeliveredAt = deliveredAt;
        FailureReason = null;
    }

    public void MarkFailed(string? reason)
    {
        if (Status.IsFailed())
        {
            return;
        }

        if (Status.IsDelivered())
        {
            throw new InvalidOperationException("A delivered delivery cannot be marked as failed.");
        }

        Status = WebhookDeliveryStatus.Failed;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            FailureReason = reason.Trim();
        }
    }
}