using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;
using WeatherMonitor.Api.Contracts;

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
                .Produces(statusCode: StatusCodes.Status401Unauthorized)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }

    private static async Task<IResult> GetMonitorsAsync(
        [AsParameters] GetMonitorsRequest request,
        [FromServices] IMediator mediator,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (principal is { Identity: null } || string.IsNullOrWhiteSpace(principal.Identity.Name))
        {
            return Results.Unauthorized();
        }

        var query = request with
        {
            ClientId = principal.Identity.Name
        };

        (MonitorResponse[] response, PagedResponse pagination) = await mediator.Send(query, cancellationToken);

        return Results.Ok(new PagedApiResponse<MonitorResponse[]>(response, pagination));
    }
}