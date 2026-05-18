using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WeatherMonitor.Api.Infrastructure.Clients.Exceptions;
using WeatherMonitor.Domain.Monitors.Exceptions;

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

    extension(MonitorCityNotFoundException exception)
    {
        internal ProblemDetails ToProblemDetails(string path)
        {
            return new ProblemDetails
            {
                Title = "Monitor City Not Found",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = path,
                Extensions = new Dictionary<string, object?> { ["trace_id"] = Activity.Current?.TraceId.ToString(), ["exception_type"] = exception.GetType().FullName }
            };
        }
    }

    extension(DuplicateWeatherMonitorException exception)
    {
        internal ProblemDetails ToProblemDetails(string path)
        {
            return new ProblemDetails
            {
                Title = "Duplicate Weather Monitor",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict,
                Instance = path,
                Extensions = new Dictionary<string, object?> { ["trace_id"] = Activity.Current?.TraceId.ToString(), ["exception_type"] = exception.GetType().FullName }
            };
        }
    }

    extension(CityLookupFailedException exception)
    {
        internal ProblemDetails ToProblemDetails(string path)
        {
            return new ProblemDetails
            {
                Title = "City Lookup Failed",
                Detail = exception.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = path,
                Extensions = new Dictionary<string, object?> { ["trace_id"] = Activity.Current?.TraceId.ToString(), ["exception_type"] = exception.GetType().FullName }
            };
        }
    }

    extension(CityLookupUnavailableException exception)
    {
        internal ProblemDetails ToProblemDetails(string path)
        {
            return new ProblemDetails
            {
                Title = "City Lookup Unavailable",
                Detail = exception.Message,
                Status = StatusCodes.Status503ServiceUnavailable,
                Instance = path,
                Extensions = new Dictionary<string, object?> { ["trace_id"] = Activity.Current?.TraceId.ToString(), ["exception_type"] = exception.GetType().FullName }
            };
        }
    }

    extension(WeatherConditionCodeNotFoundException exception)
    {
        internal ProblemDetails ToProblemDetails(string path)
        {
            return new ProblemDetails
            {
                Title = "Weather Condition Not Found",
                Detail = exception.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = path,
                Extensions = new Dictionary<string, object?> { ["trace_id"] = Activity.Current?.TraceId.ToString(), ["exception_type"] = exception.GetType().FullName }
            };
        }
    }
    
    extension(MonitorNotFoundException exception)
    {
        internal ProblemDetails ToProblemDetails(string path)
        {
            return new ProblemDetails
            {
                Title = "Monitor Not Found",
                Detail = exception.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = path,
                Extensions = new Dictionary<string, object?> { ["trace_id"] = Activity.Current?.TraceId.ToString(), ["exception_type"] = exception.GetType().FullName }
            };
        }
    }
}