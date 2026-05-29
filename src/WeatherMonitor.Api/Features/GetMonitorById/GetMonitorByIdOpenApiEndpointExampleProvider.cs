using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.OpenApi.Providers;
using WeatherMonitor.Domain.Monitors.ValueObjects;

namespace WeatherMonitor.Api.Features.GetMonitorById;

internal sealed class GetMonitorByIdOpenApiEndpointExampleProvider : OpenApiEndpointExampleProvider
{
    private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public override OpenApiMediaTypeExample Create200OkExample()
    {
        var monitor = new MonitorResponse
        {
            MonitorId = Guid.Parse("8b2f1d5d-7c6f-43a2-8d7e-85f0f87e8f91"),
            CityCode = 244,
            CityName = BrazilianState.SaoPaulo.Name,
            State = BrazilianState.SaoPaulo.Value,
            WeatherConditionCode = WeatherCondition.Drizzle.Code,
            WeatherConditionDescription = WeatherCondition.Drizzle.Description,
            WebhookUrl = "http://samples-nodejs/api/external/weather",
            Time = TimeOnly.Parse("08:30:00"),
            TimeZoneId = TimeZone.Id,
            CreatedAt = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZone),
            UpdatedAt = null,
            Enabled = true
        };

        var response = new ApiResponse<MonitorResponse>(monitor);

        return CreateExampleEntry(
            key: "get-monitor",
            contentType: MediaTypeNames.Application.Json,
            summary: "Monitor returned by id",
            description: "Returns the weather monitor using the provided ID.",
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
                ["errors"] = new Dictionary<string, string[]> { ["monitorId"] = ["must not be empty", "must not be null"] }
            }
        };

        return CreateBadRequestExampleEntry(problemDetails);
    }

    public override OpenApiMediaTypeExample Create404NotFoundExample(string? path)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Monitor Not Found",
            Detail = "Monitor with ID {monitorId} was not found for the given client ID.",
            Status = StatusCodes.Status404NotFound,
            Instance = path,
            Extensions = new Dictionary<string, object?> { ["trace_id"] = Guid.NewGuid().ToString(), ["exception_type"] = nameof(MonitorNotFoundException) }
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