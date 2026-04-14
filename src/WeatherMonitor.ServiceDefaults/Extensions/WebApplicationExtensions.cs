using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace WeatherMonitor.ServiceDefaults.Extensions;

public static class WebApplicationExtensions
{
    private const string ReadinessEndpointPath = "/healthz/ready";
    private const string LivenessEndpointPath = "/healthz/live";

    extension(WebApplication app)
    {
        public WebApplication MapDefaultEndpoints()
        {
            if (!app.Environment.IsDevelopment())
            {
                return app;
            }

            app.MapHealthChecks(ReadinessEndpointPath, WebApplication.CreateHealthCheckOptions(tag: "ready"));

            app.MapHealthChecks(LivenessEndpointPath, WebApplication.CreateHealthCheckOptions(tag: "alive"));

            return app;
        }

        private static HealthCheckOptions CreateHealthCheckOptions(string tag)
        {
            return new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(tag),
                ResponseWriter = (context, report) => context.HealthCheckResponseWriter(report),
                AllowCachingResponses = false
            };
        }
    }
}