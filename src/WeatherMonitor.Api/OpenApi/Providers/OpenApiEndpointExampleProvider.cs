using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WeatherMonitor.Api.OpenApi.Providers;

internal abstract class OpenApiEndpointExampleProvider : IOpenApiEndpointExampleProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static JsonNode? SerializeExampleNode<TValue>(TValue value)
    {
        return JsonSerializer.SerializeToNode(value, JsonOptions);
    }

    protected static OpenApiMediaTypeExample CreateExampleEntry<TValue>(string key, string contentType, string summary, string description, TValue value)
    {
        var node = SerializeExampleNode(value);

        var example = new OpenApiExample { Summary = summary, Description = description, Value = node };

        return new OpenApiMediaTypeExample(key, contentType, example);
    }

    protected static OpenApiMediaTypeExample CreateBadRequestExampleEntry(ProblemDetails problemDetails)
    {
        return CreateExampleEntry(
            key: "bad-request",
            contentType: MediaTypeNames.Application.ProblemJson,
            summary: "Unexpected input from client.",
            description: "HTTP Status Code 400 - Bad Request",
            value: problemDetails
        );
    }

    public virtual OpenApiMediaTypeExample? CreateBodyExample() => null;

    public virtual OpenApiMediaTypeExample? Create200OkExample() => null;

    public virtual OpenApiMediaTypeExample? Create201CreatedExample() => null;

    public abstract OpenApiMediaTypeExample Create400BadRequestExample(string? path);

    public virtual OpenApiMediaTypeExample Create404NotFoundExample(string? path)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Not Found",
            Detail = "The requested resource was not found.",
            Status = StatusCodes.Status404NotFound,
            Instance = path,
            Extensions = new Dictionary<string, object?> { ["trace_id"] = Guid.NewGuid().ToString(), ["exception_type"] = "System.Exception" }
        };

        return CreateExampleEntry(
            key: "not-found",
            contentType: MediaTypeNames.Application.ProblemJson,
            summary: "Target resource not found.",
            description: "HTTP Status Code 404 - Not Found",
            value: problemDetails
        );
    }

    public virtual OpenApiMediaTypeExample Create409ConflictExample(string? path)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Conflict",
            Detail = "The request conflicts with the current resource state.",
            Status = StatusCodes.Status409Conflict,
            Instance = path,
            Extensions = new Dictionary<string, object?> { ["trace_id"] = Guid.NewGuid().ToString(), ["exception_type"] = "System.Exception" }
        };

        return CreateExampleEntry(
            key: "conflict",
            contentType: MediaTypeNames.Application.ProblemJson,
            summary: "Entity already exists.",
            description: "HTTP Status Code 409 - Conflict",
            value: problemDetails
        );
    }

    public virtual OpenApiMediaTypeExample Create500InternalServerErrorExample(string? path)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred while processing the request.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = path,
            Extensions = new Dictionary<string, object?> { ["trace_id"] = Guid.NewGuid().ToString(), ["exception_type"] = "System.Exception" }
        };

        return CreateExampleEntry(
            key: "internal-server-error",
            contentType: MediaTypeNames.Application.ProblemJson,
            summary: "Unexpected error in the server.",
            description: "HTTP Status Code 500 - Internal Server Error",
            value: problemDetails
        );
    }

    public virtual OpenApiMediaTypeExample Create503ServiceUnavailableExample(string? path)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "Service Unavailable",
            Status = StatusCodes.Status503ServiceUnavailable,
            Detail = "The service is temporarily unavailable.",
            Instance = path,
            Extensions = new Dictionary<string, object?> { ["trace_id"] = Guid.NewGuid().ToString(), ["exception_type"] = "System.Exception" }
        };

        return CreateExampleEntry(
            key: "service-unavailable",
            contentType: MediaTypeNames.Application.ProblemJson,
            summary: "External dependency is unavailable.",
            description: "HTTP Status Code 503 - Service Unavailable",
            value: problemDetails
        );
    }
}