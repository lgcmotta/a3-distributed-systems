using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.GetWeatherConditionByCode;

internal static class GetWeatherConditionByCodeEndpoint
{
    extension(IEndpointRouteBuilder builder)
    {
        internal IEndpointRouteBuilder MapGetWeatherConditionByCodeEndpoint(ApiVersion version)
        {
            builder.MapGet("weather-condition-codes/{code}", GetWeatherConditionByCodeAsync)
                .WithName($"get-v{version:V}-weather-condition-codes-code")
                .WithDisplayName("Get Weather Condition By Code")
                .WithTags("weather")
                .RequireAuthorization()
                .Produces<ApiResponse<WeatherConditionByCodeResponse>>(contentType: MediaTypeNames.Application.Json)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }
    
    private static async Task<IResult> GetWeatherConditionByCodeAsync(
        [AsParameters] GetWeatherConditionByCodeRequest query,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        WeatherConditionByCodeResponse response = await mediator.Send(query, cancellationToken);

        return Results.Ok(new ApiResponse<WeatherConditionByCodeResponse>(response));
    }
}