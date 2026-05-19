using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WeatherMonitor.Api.Contracts;

namespace WeatherMonitor.Api.Features.GetWeatherConditions;

internal static class GetWeatherConditionsEndpoint
{
    extension(IEndpointRouteBuilder builder)
    {
        internal IEndpointRouteBuilder MapGetWeatherConditionCodes(ApiVersion version)
        {
            builder.MapGet("weather-condition-codes", GetWeatherConditionCodesAsync)
                .WithName($"get-v{version:V}-weather-condition-codes")
                .WithDisplayName("Get Paginated Weather Conditions")
                .WithTags("weather")
                .RequireAuthorization()
                .Produces<PagedApiResponse<WeatherConditionResponse[]>>(contentType: MediaTypeNames.Application.Json)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .Produces(statusCode: StatusCodes.Status401Unauthorized)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }

    private static async Task<IResult> GetWeatherConditionCodesAsync(
        [AsParameters] WeatherConditionRequest query,
        IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        (WeatherConditionResponse[] response, PagedResponse pagination) = await mediator.Send(query, cancellationToken);

        return Results.Ok(new PagedApiResponse<WeatherConditionResponse[]>(response, pagination));
    }
}