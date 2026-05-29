using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.OpenApi.Providers;
using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Monitors.ValueObjects;

namespace WeatherMonitor.Api.Features.GetWeatherConditions;

internal sealed class GetWeatherConditionsOpenApiEndpointExampleProvider : OpenApiEndpointExampleProvider
{
    public override OpenApiMediaTypeExample Create200OkExample()
    {
        WeatherCondition[] conditions = [.. Enumeration.Enumerate<WeatherCondition>()];

        var response = new PagedApiResponse<WeatherConditionResponse[]>(
            Data:
            [
                ..conditions.OrderBy(condition => condition.Code, StringComparer.Ordinal)
                    .Take(5)
                    .Select(condition => new WeatherConditionResponse(condition.Code, condition.Description))
            ],
            Pagination: new PagedResponse
            {
                Page = 1,
                Size = 5,
                Previous = 0,
                Next = 2,
                Total = conditions.Length,
                TotalPages = 1
            }
        );

        return CreateExampleEntry(
            key: "get-weather-conditions",
            contentType: MediaTypeNames.Application.Json,
            summary: "Paginated weather conditions returned.",
            description: "Returns paginated weather conditions.",
            value: response
        );
    }

    public override OpenApiMediaTypeExample Create400BadRequestExample(string? path)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Validation Error",
            Detail = "One or more properties have errors",
            Status = StatusCodes.Status400BadRequest,
            Instance = path,
            Extensions = new Dictionary<string, object?>
            {
                ["trace_id"] = Guid.NewGuid().ToString(),
                ["exception_type"] = nameof(ValidationException),
                ["errors"] = new Dictionary<string, string[]>
                {
                    ["page"] = ["must be greater than 0"],
                    ["size"] = ["must be greater than 0", "must be less than or equal to 50"]
                }
            }
        };

        return CreateBadRequestExampleEntry(problemDetails);
    }
}