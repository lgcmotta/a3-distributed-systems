using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Claims;
using WeatherMonitor.Api.Shared;

namespace WeatherMonitor.Api.Features.CreateMonitor;

internal static class CreateMonitorEndpoint
{
    extension(IEndpointRouteBuilder builder)
    {
        internal IEndpointRouteBuilder MapPostCreateMonitor(ApiVersion version)
        {
            builder.MapPost("monitors", PostCreateMonitorAsync)
                .WithName($"post-v{version:V}-monitors")
                .WithDisplayName("Create Monitor")
                .WithTags("monitors")
                .RequireAuthorization()
                .Produces<ApiResponse<MonitorResponse>>(StatusCodes.Status201Created, contentType: MediaTypeNames.Application.Json)
                .Produces<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, MediaTypeNames.Application.ProblemJson)
                .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError, MediaTypeNames.Application.ProblemJson);

            return builder;
        }
    }

    private static async Task<IResult> PostCreateMonitorAsync(
        [FromBody] CreateMonitorRequest body,
        IMediator mediator,
        ApiVersion version,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        CreateMonitorRequest command = body with { ClientId = principal.Identity!.Name ?? string.Empty };

        var response = await mediator.Send(command, cancellationToken);

        return Results.Created($"/api/v{version:V}/monitors/{response.MonitorId}", new ApiResponse<MonitorResponse>(response));
    }
}