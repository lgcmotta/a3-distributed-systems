using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.GetWeatherConditionCodes;

internal static class GetWeatherConditionCodesEndpoint
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
                .Produces<PagedApiResponse<IEnumerable<WeatherConditionResponse>>>(contentType: MediaTypeNames.Application.Json)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }

    private static async Task<IResult> GetWeatherConditionCodesAsync(
        [AsParameters] GetWeatherConditionsQuery query,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken = default)
    {
        (IEnumerable<WeatherConditionResponse> response, PagedResponseModel pagination) = await mediator.Send(query, cancellationToken);

        return Results.Ok(new PagedApiResponse<IEnumerable<WeatherConditionResponse>>(response, pagination));
    }
}