namespace WeatherMonitor.Api.Features.MonitorProcessing;

public record WeatherMonitorProcessorOptions
{
    public const string SectionName = "WeatherMonitorProcessor";

    public const string DefaultCronExpression = "0 0 * * *";

    public required string ProcessorCronExpression { get; init; }

    public int MaxConcurrency { get; init; } = 1;

    internal static WeatherMonitorProcessorOptions Default() => new() { ProcessorCronExpression = DefaultCronExpression };
}