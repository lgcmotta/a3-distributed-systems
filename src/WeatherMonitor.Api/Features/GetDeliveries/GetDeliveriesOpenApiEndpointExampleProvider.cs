using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WeatherMonitor.Api.OpenApi.Providers;
using WeatherMonitor.Domain.Monitors.ValueObjects;
using WeatherMonitor.Api.Contracts;
using System.Net.Mime;

namespace WeatherMonitor.Api.Features.GetDeliveries;

internal sealed class GetDeliveriesOpenApiEndpointExampleProvider : OpenApiEndpointExampleProvider
{
    private static readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public override OpenApiMediaTypeExample Create200OkExample()
    {
        var response = new PagedApiResponse<DeliveryResponse[]>(
            Data:
            [
                new DeliveryResponse
                {
                    DeliveryId = Guid.Parse("8b2f1d5d-7c6f-43a2-8d7e-85f0f87e8f91"),
                    MonitorId = Guid.Parse("4e7f6d44-55e2-4cb1-9d4c-2f8b66b82e10"),
                    ForecastDate = new DateOnly(2026, 05, 24),
                    ScheduledFor = DateTimeOffset.Parse("2026-05-24T08:30:00-03:00"),
                    DeliveredAt = DateTimeOffset.Parse("2026-05-24T08:30:05-03:00"),
                    Status = "Delivered",
                    RetryCount = 0,
                    FailureReason = null,
                    CityCode = 244,
                    CityName = BrazilianState.SaoPaulo.Name,
                    State = BrazilianState.SaoPaulo.Value,
                    WeatherConditionCode = WeatherCondition.Drizzle.Code,
                    WeatherConditionDescription = WeatherCondition.Drizzle.Description,
                    TimeZoneId = TimeZone.Id,
                    CreatedAt = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZone),
                    UpdatedAt = null,
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
            key: "get-deliveries",
            contentType: MediaTypeNames.Application.Json,
            summary: "Deliveries returned",
            description: "Returns the deliveries.",
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
                ["errors"] = new Dictionary<string, string[]> { ["page"] = ["must be greater than 0"], ["size"] = ["must be greater than 0", "must be less than or equal to 50"], ["start"] = ["start must be less than or equal to end"] }
            }
        };

        return CreateBadRequestExampleEntry(problemDetails);
    }
}