using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;
using WeatherMonitor.Api.Contracts;

namespace WeatherMonitor.Api.Features.GetMonitorById;

internal static class GetMonitorByIdEndpoint
{
    extension(IEndpointRouteBuilder builder)
    {
        internal IEndpointRouteBuilder MapGetMonitorById(ApiVersion version)
        {
            builder.MapGet("monitors/{monitorId:guid}", GetMonitorByIdAsync)
                .WithName($"get-v{version:V}-monitors-monitor-id")
                .WithDisplayName("Get Monitor By Id")
                .WithTags("monitors")
                .RequireAuthorization()
                .Produces<ApiResponse<MonitorResponse>>(contentType: MediaTypeNames.Application.Json)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .Produces(statusCode: StatusCodes.Status401Unauthorized)
                .Produces<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }

    private static async Task<IResult> GetMonitorByIdAsync(
        Guid monitorId,
        IMediator mediator,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (principal is { Identity: null } || string.IsNullOrWhiteSpace(principal.Identity.Name))
        {
            return Results.Unauthorized();
        }

        var query = new GetMonitorByIdQuery(monitorId, principal.Identity.Name);

        MonitorResponse result = await mediator.Send(query, cancellationToken);

        return Results.Ok(new ApiResponse<MonitorResponse>(result));
    }
}