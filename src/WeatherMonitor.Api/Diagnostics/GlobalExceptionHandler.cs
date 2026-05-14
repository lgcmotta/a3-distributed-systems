using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mime;
using System.Text.Json;
using WeatherMonitor.Api.Extensions;
using WeatherMonitor.Api.Infrastructure.Clients.Exceptions;
using WeatherMonitor.Domain.Monitors.Exceptions;

namespace WeatherMonitor.Api.Diagnostics;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var instance = httpContext.Request.Path.Value ?? string.Empty;

        var problemDetails = exception switch
        {
            ValidationException e => e.ToProblemDetails(instance),
            MonitorCityNotFoundException e => e.ToProblemDetails(instance),
            DuplicateWeatherMonitorException e => e.ToProblemDetails(instance),
            CityLookupFailedException e => e.ToProblemDetails(instance),
            CityLookupUnavailableException e => e.ToProblemDetails(instance),
            ArgumentNullException e => e.ToProblemDetails(instance),
            ArgumentOutOfRangeException e => e.ToProblemDetails(instance),
            ArgumentException e => e.ToProblemDetails(instance),
            WeatherConditionCodeNotFoundException e => e.ToProblemDetails(instance),
            _ => exception.ToProblemDetails(instance)
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, JsonSerializerOptions.Web, cancellationToken);

        return true;
    }
}