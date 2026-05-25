using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;
using WeatherMonitor.Api.Contracts;
using WeatherMonitor.Api.OpenApi;

namespace WeatherMonitor.Api.Features.UpdateMonitor;

internal static class PatchMonitorEndpoint
{
    extension(IEndpointRouteBuilder builder)
    {
        internal IEndpointRouteBuilder MapPatchUpdateMonitor(ApiVersion version)
        {
            builder.MapPatch("/monitors/{monitorId:guid}", PatchMonitorAsync)
                .WithName($"patch-v{version:V}-monitors-monitor-id")
                .WithDisplayName("Patch Monitor")
                .WithTags("monitors")
                .RequireAuthorization()
                .WithOpenApiExampleProvider<PatchMonitorOpenApiEndpointExampleProvider>()
                .Produces<ApiResponse<MonitorResponse>>(contentType: MediaTypeNames.Application.Json)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .Produces(statusCode: StatusCodes.Status401Unauthorized)
                .Produces<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }

    private static async Task<IResult> PatchMonitorAsync(
        [FromRoute] Guid monitorId,
        [FromBody] PatchMonitorRequest body,
        IMediator mediator,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        if (principal is { Identity: null } || string.IsNullOrWhiteSpace(principal.Identity.Name))
        {
            return Results.Unauthorized();
        }

        PatchMonitorRequest command = body with { ClientId = principal.Identity.Name, MonitorId = monitorId };

        MonitorResponse response = await mediator.Send(command, cancellationToken);

        return Results.Ok(new ApiResponse<MonitorResponse>(response));
    }
}