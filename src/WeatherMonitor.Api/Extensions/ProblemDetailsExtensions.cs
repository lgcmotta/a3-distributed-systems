using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace WeatherMonitor.Api.Extensions;

internal static class ProblemDetailsExtensions
{
    extension(Exception exception)
    {
        internal ProblemDetails ToProblemDetails(string path)
        {
            return new ProblemDetails
            {
                Title = "Unknown Internal Error",
                Detail = "An error occurred while processing the http request. Please, contact an administrator.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = path,
                Extensions = new Dictionary<string, object?>
                {
                    ["trace_id"] = Activity.Current?.TraceId.ToString(),
                    ["exception_type"] = exception.GetType().FullName
                }
            };
        }
    }

    extension(ValidationException exception)
    {
        internal ProblemDetails ToProblemDetails(string path)
        {
            return new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more properties have errors",
                Instance = path,
                Extensions =
                {
                    ["trace_id"] = Activity.Current?.TraceId.ToString(),
                    ["exception_type"] = exception.GetType().FullName,
                    ["errors"] = exception.Errors
                        .GroupBy(failure => JsonNamingPolicy.CamelCase.ConvertName(failure.PropertyName))
                        .ToDictionary(
                            grouping => grouping.Key,
                            grouping => grouping.Select(failure => failure.ErrorMessage).ToArray()
                        )
                }
            };
        }
    }
}