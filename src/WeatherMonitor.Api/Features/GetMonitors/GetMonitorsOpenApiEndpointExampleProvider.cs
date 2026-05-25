using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Api.OpenApi.Providers;
using WeatherMonitor.Domain.Monitors.ValueObjects;
using WeatherMonitor.Api.Contracts;
using System.Net.Mime;

namespace WeatherMonitor.Api.Features.GetMonitors;

internal sealed class GetMonitorsOpenApiEndpointExampleProvider : OpenApiEndpointExampleProvider
{
    private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public override OpenApiMediaTypeExample Create200OkExample()
    {
        var response = new PagedApiResponse<MonitorResponse[]>(
            Data:
            [
                new MonitorResponse
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
                }
            ],
            Pagination: new PagedResponse
            {
                Page = 1,
                Size = 10,
                Previous = 0,
                Next = 0,
                Total = 1,
                TotalPages = 1
            }
        );

        return CreateExampleEntry(
            key: "get-monitors",
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
                ["errors"] = new Dictionary<string, string[]> { ["page"] = ["must be greater than 0"], ["size"] = ["must be greater than 0", "must be less than or equal to 50"] }
            }
        };

        return CreateBadRequestExampleEntry(problemDetails);
    }
}