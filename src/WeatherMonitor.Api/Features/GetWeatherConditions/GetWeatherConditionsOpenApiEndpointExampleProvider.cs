using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Api.OpenApi.Providers;
using WeatherMonitor.Api.Contracts;

using System.Net.Mime;

namespace WeatherMonitor.Api.Features.GetWeatherConditions;

internal sealed class GetWeatherConditionsOpenApiEndpointExampleProvider : OpenApiEndpointExampleProvider
{
    public override OpenApiMediaTypeExample Create200OkExample()
    {
        var response = new PagedApiResponse<WeatherConditionResponse[]>(
            Data:
            [
                new WeatherConditionResponse("c", "Chuva"),
                new WeatherConditionResponse("ch", "Chuvoso"),
                new WeatherConditionResponse("ci", "Chuvas Isoladas"),
                new WeatherConditionResponse("cl", "Céu Claro"),
                new WeatherConditionResponse("cm", "Chuva pela Manhã"),
                new WeatherConditionResponse("cn", "Chuva a Noite"),
                new WeatherConditionResponse("ct", "Chuva a Tarde"),
                new WeatherConditionResponse("cv", "Chuvisco"),
                new WeatherConditionResponse("e", "Encoberto"),
                new WeatherConditionResponse("ec", "Encoberto com Chuvas Isoladas"),
            ],
            Pagination: new PagedResponse
            {
                Page = 1,
                Size = 10,
                Previous = 0,
                Next = 0,
                Total = 10,
                TotalPages = 1
            }
        );

        return CreateExampleEntry(
            key: "get-weather-conditions",
            contentType: MediaTypeNames.Application.Json,
            summary: "Weather conditions returned by code",
            description: "Returns the weather conditions using the provided code.",
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
                ["errors"] = new Dictionary<string, string[]> { ["page"] = ["must be greater than 0"], ["size"] = ["must be greater than 0", "must be less than or equal to 50"] }
            }
        };

        return CreateBadRequestExampleEntry(problemDetails);
    }
}