using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;
using WeatherMonitor.Api.Contracts;

namespace WeatherMonitor.Api.Features.GetDeliveries;

internal static class GetDeliveriesEndpoint
{
    extension(IEndpointRouteBuilder builder)
    {
        internal IEndpointRouteBuilder MapGetDeliveries(ApiVersion version)
        {
            builder.MapGet("deliveries", GetDeliveriesAsync)
                .WithName($"get-v{version:V}-deliveries")
                .WithDisplayName("Get Paginated Deliveries")
                .WithTags("deliveries")
                .RequireAuthorization()
                .Produces<PagedApiResponse<DeliveryResponse[]>>(contentType: MediaTypeNames.Application.Json)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .Produces(statusCode: StatusCodes.Status401Unauthorized)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }

    private static async Task<IResult> GetDeliveriesAsync(
        [AsParameters] GetDeliveriesRequest request,
        IMediator mediator,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (principal is { Identity: null } || string.IsNullOrWhiteSpace(principal.Identity?.Name))
        {
            return Results.Unauthorized();
        }

        var query = request with
        {
            ClientId = principal.Identity.Name
        };

        (DeliveryResponse[] response, PagedResponse pagination) = await mediator.Send(query, cancellationToken);

        return Results.Ok(new PagedApiResponse<DeliveryResponse[]>(response, pagination));
    }
}
