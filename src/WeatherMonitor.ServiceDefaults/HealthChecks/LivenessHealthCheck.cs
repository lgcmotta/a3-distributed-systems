using JetBrains.Annotations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace WeatherMonitor.ServiceDefaults.HealthChecks;

[UsedImplicitly]
public class LivenessHealthCheck : IHealthCheck
{
    private volatile bool _isReady;

    public bool StartupCompleted
    {
        get => _isReady;
        set => _isReady = value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var result = StartupCompleted
            ? HealthCheckResult.Healthy("The startup task has completed.")
            : HealthCheckResult.Unhealthy("That startup task is still running.");

        return Task.FromResult(result);
    }
}

public sealed class LivenessBackgroundService(LivenessHealthCheck check) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        check.StartupCompleted = true;

        return Task.CompletedTask;
    }
}