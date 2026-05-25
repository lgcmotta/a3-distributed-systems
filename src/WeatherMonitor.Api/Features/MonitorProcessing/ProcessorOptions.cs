namespace WeatherMonitor.Api.Features.MonitorProcessing;

public record ProcessorOptions
{
    public const string SectionName = "WeatherMonitorProcessor";
    public const string DefaultCronExpression = "0 0 * * *";

    public required string CronExpression { get; init; }
    public int MaxConcurrency { get; init; } = 1;
    public int ForecastDays { get; init; } = 2;

    internal static ProcessorOptions Default() => new() { CronExpression = DefaultCronExpression };
}