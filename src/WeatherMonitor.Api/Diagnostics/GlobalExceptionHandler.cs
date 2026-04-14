using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mime;
using System.Text.Json;
using WeatherMonitor.Api.Extensions;

namespace WeatherMonitor.Api.Diagnostics;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var instance = httpContext.Request.Path.Value ?? string.Empty;

        var problemDetails = exception switch
        {
            ValidationException e => e.ToProblemDetails(instance),
            ArgumentNullException e => e.ToProblemDetails(instance),
            ArgumentOutOfRangeException e => e.ToProblemDetails(instance),
            ArgumentException e => e.ToProblemDetails(instance),
            // TODO: add missing or custom exception cases here
            _ => exception.ToProblemDetails(instance)
        };

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, JsonSerializerOptions.Web, cancellationToken);

        return true;
    }
}