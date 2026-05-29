using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.OpenApi.Providers;
using WeatherMonitor.Domain.Monitors.Exceptions;
using WeatherMonitor.Domain.Monitors.ValueObjects;

namespace WeatherMonitor.Api.Features.UpdateMonitor;

internal sealed class PatchMonitorOpenApiEndpointExampleProvider : OpenApiEndpointExampleProvider
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
            UpdatedAt = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZone),
            Enabled = true
        };

        var response = new ApiResponse<MonitorResponse>(monitor);

        return CreateExampleEntry(
            key: "update-monitor",
            contentType: MediaTypeNames.Application.Json,
            summary: "Monitor returned",
            description: "Returns the configured weather monitor.",
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
                    ["body"] = ["must include at least one property to patch"],
                    ["monitorId"] = ["cannot be empty or null"],
                    ["webhookUrl"] = ["must not be empty", "must be a well-formed absolute URI", "must use HTTP or HTTPS scheme"]
                }
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
            Extensions = new Dictionary<string, object?>
            {
                ["trace_id"] = Guid.NewGuid().ToString(),
                ["exception_type"] = nameof(MonitorNotFoundException),
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