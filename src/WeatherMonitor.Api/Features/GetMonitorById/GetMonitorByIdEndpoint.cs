using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.GetMonitorById;

internal static class GetMonitorByIdEndpoint
{
    extension(IEndpointRouteBuilder builder)
    {
        internal IEndpointRouteBuilder MapGetMonitorById(ApiVersion version)
        {
            builder.MapGet("monitors/{monitorId}", GetMonitorByIdAsync)
                .WithName($"post-v{version:V}-monitors-monitor-id")
                .WithDisplayName("Get Monitor By Id")
                .WithTags("monitors")
                .RequireAuthorization()
                .Produces<ApiResponse<MonitorResponse>>(contentType: MediaTypeNames.Application.Json)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }

    private static async Task<IResult> GetMonitorByIdAsync(
        [AsParameters] GetMonitorByIdRequest query,
        [FromServices] IMediator mediator,
        ApiVersion version,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (principal.Identity is null || string.IsNullOrWhiteSpace(principal.Identity.Name))
            return Results.Unauthorized();

        GetMonitorByIdRequest command = query with { ClientId = principal.Identity.Name };
        var result = await mediator.Send(command, cancellationToken);
        
        return Results.Ok(new ApiResponse<MonitorResponse>(result));
    }
}