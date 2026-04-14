using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Mime;
using System.Text.Json;

namespace WeatherMonitor.ServiceDefaults.Extensions;

internal static class HttpContextExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) { PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower };

    extension(HttpContext context)
    {
        internal bool IsNotHealthCheck()
        {
            return !context.Request.Path.StartsWithSegments("/healthz/ready")
                   && !context.Request.Path.StartsWithSegments("/healthz/live");
        }

        internal async Task HealthCheckResponseWriter(HealthReport report)
        {
            int statusCode = report.Status switch
            {
                HealthStatus.Unhealthy => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status200OK,
            };

            var response = new
            {
                Status = report.Status.ToString(),
                TotalDuration = report.TotalDuration.ToString("c"),
                Entries = report.Entries.ToDictionary(entry => JsonNamingPolicy.SnakeCaseLower.ConvertName(entry.Key))
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            await context.Response.WriteAsJsonAsync(response, Options);
        }
    }
}