using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.GetMonitors;

internal static class GetMonitorsEndpoint
{
    extension(IEndpointRouteBuilder builder)
    {
        internal IEndpointRouteBuilder MapGetMonitors(ApiVersion version)
        {
            builder.MapGet("monitors", GetMonitorsAsync)
                .WithName($"get-v{version:V}-monitors")
                .WithDisplayName("Get Paginated Monitors")
                .WithTags("monitors")
                .RequireAuthorization()
                .Produces<PagedApiResponse<MonitorResponse[]>>(contentType: MediaTypeNames.Application.Json)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }

    private static async Task<IResult> GetMonitorsAsync(
        [AsParameters] GetMonitorsRequest query,
        [FromServices] IMediator mediator,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken cancellationToken = default)
    {
        if (claimsPrincipal.Identity is null || string.IsNullOrWhiteSpace(claimsPrincipal.Identity.Name))
        {
            return Results.Unauthorized();
        }

        var request = query with
        {
            ClientId = claimsPrincipal.Identity.Name
        };

        (IEnumerable<MonitorResponse> response, PagedResponse pagination) = await mediator.Send(request, cancellationToken);

        return Results.Ok(new PagedApiResponse<IEnumerable<MonitorResponse>>(response, pagination));
    }
}