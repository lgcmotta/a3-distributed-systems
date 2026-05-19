namespace WeatherMonitor.Api.Infrastructure.Extensions;

internal static class DateTimeOffsetExtensions
{
    extension(DateTimeOffset? value)
    {
        internal DateTimeOffset? ToLocalTimeZone(string timeZoneId)
        {
            return value?.ToLocalTimeZone(timeZoneId);
        }
    }

    extension(DateTimeOffset value)
    {
        internal DateTimeOffset ToLocalTimeZone(string timeZoneId)
        {
            return TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var timeZone)
                ? TimeZoneInfo.ConvertTime(value, timeZone)
                : value;
        }
    }
}