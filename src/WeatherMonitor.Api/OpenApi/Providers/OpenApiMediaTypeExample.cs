using Microsoft.OpenApi;

namespace WeatherMonitor.Api.OpenApi.Providers;

internal sealed record OpenApiMediaTypeExample(string Key, string ContentType, OpenApiExample Example);