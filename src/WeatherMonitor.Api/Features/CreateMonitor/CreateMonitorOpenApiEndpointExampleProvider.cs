using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.OpenApi.Providers;
using WeatherMonitor.Domain.Monitors.ValueObjects;

namespace WeatherMonitor.Api.Features.CreateMonitor;

internal sealed class CreateMonitorOpenApiEndpointExampleProvider : OpenApiEndpointExampleProvider
{
    private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public override OpenApiMediaTypeExample CreateBodyExample()
    {
        var body = new CreateMonitorRequest(
            ClientId: string.Empty, // The ClientId will be ignored when serialized.
            City: BrazilianState.SaoPaulo.Name,
            State: BrazilianState.SaoPaulo.Value,
            WeatherConditionCode: WeatherCondition.Storm.Code,
            Time: TimeOnly.Parse("08:30:00"),
            TimeZoneId: TimeZone.Id,
            WebhookUrl: "http://samples-nodejs/api/external/weather",
            AccessToken: Guid.NewGuid().ToString()
        );

        return CreateExampleEntry(
            key: "create-monitor",
            contentType: MediaTypeNames.Application.Json,
            summary: "Create monitor",
            description: "Creates a weather monitor for a city, weather condition, schedule and webhook target.",
            value: body);
    }

    public override OpenApiMediaTypeExample Create201CreatedExample()
    {
        var monitor = new MonitorResponse
        {
            MonitorId = Guid.Parse("8b2f1d5d-7c6f-43a2-8d7e-85f0f87e8f91"),
            CityCode = 3550308,
            CityName = BrazilianState.SaoPaulo.Name,
            State = BrazilianState.SaoPaulo.Value,
            WeatherConditionCode = WeatherCondition.Storm.Code,
            WeatherConditionDescription = WeatherCondition.Storm.Description,
            WebhookUrl = "http://samples-nodejs/api/external/weather",
            Time = TimeOnly.Parse("08:30:00"),
            TimeZoneId = TimeZone.Id,
            CreatedAt = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZone),
            UpdatedAt = null,
            Enabled = true
        };

        var response = new ApiResponse<MonitorResponse>(monitor);

        return CreateExampleEntry(
            key: "monitor",
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
            Title = "Bad Request",
            Detail = "The request could not be processed.",
            Status = StatusCodes.Status400BadRequest,
            Instance = path,
            Extensions = new Dictionary<string, object?>
            {
                ["trace_id"] = Guid.NewGuid().ToString(),
                ["exception_type"] = nameof(ValidationException),
                ["errors"] = new Dictionary<string, string[]> { ["webhookUrl"] = ["must be a well-formed absolute URI", "must use HTTP or HTTPS scheme"] }
            }
        };

        return CreateBadRequestExampleEntry(problemDetails);
    }
}