using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.OpenApi.Providers;

namespace WeatherMonitor.Api.Features.GetWeatherConditionByCode;

internal sealed class GetWeatherConditionByCodeOpenApiEndpointExampleProvider : OpenApiEndpointExampleProvider
{
    public override OpenApiMediaTypeExample Create200OkExample()
    {
        var weatherCondition = new WeatherConditionResponse("cv", "chuvisco");

        var response = new ApiResponse<WeatherConditionResponse>(weatherCondition);

        return CreateExampleEntry(
            key: "get-weather-condition",
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
                ["errors"] = new Dictionary<string, string[]> { ["code"] = ["cannot be empty or null"] }
            }
        };

        return CreateBadRequestExampleEntry(problemDetails);
    }

    public override OpenApiMediaTypeExample Create404NotFoundExample(string? path)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Weather Condition Not Found",
            Detail = "No weather conditions found for code: {code}",
            Status = StatusCodes.Status404NotFound,
            Instance = path,
            Extensions = new Dictionary<string, object?>
            {
                ["trace_id"] = Guid.NewGuid().ToString(),
                ["exception_type"] = nameof(SystemException),
            }
        };

        return CreateExampleEntry(
          key: "not-found",
          contentType: MediaTypeNames.Application.ProblemJson,
          summary: "Target resource not found.",
          description: "HTTP Status Code 404 - Not Found",
          value: problemDetails
      );
    }
}