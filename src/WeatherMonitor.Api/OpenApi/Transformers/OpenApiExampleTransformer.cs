using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using WeatherMonitor.Api.OpenApi.Providers;

namespace WeatherMonitor.Api.OpenApi.Transformers;

internal class OpenApiExampleTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken _)
    {
        var providers = context.Description.ActionDescriptor.EndpointMetadata.OfType<IOpenApiEndpointExampleProvider>();

        foreach (IOpenApiEndpointExampleProvider provider in providers)
        {
            if (operation is { RequestBody.Content: not null })
            {
                var requestExample = provider.CreateBodyExample();

                if (requestExample is not null && (operation.RequestBody?.Content.TryGetValue(requestExample.ContentType, out var requestMediaType) ?? false))
                {
                    requestMediaType.Examples ??= new Dictionary<string, IOpenApiExample>();

                    if (!requestMediaType.Examples.TryAdd(requestExample.Key, requestExample.Example))
                    {
                        throw new InvalidOperationException(
                            $"""
                             Duplicated OpenAPI request example key '{requestExample.Key}' for content type '{requestExample.ContentType}'. 
                             Use a unique example key for each request body example on the same endpoint.
                             """
                        );
                    }
                }
            }

            foreach ((string statusCode, IOpenApiResponse response) in operation.Responses ?? [])
            {
                if (response is { Content: null })
                {
                    continue;
                }

                var responseExample = int.Parse(statusCode) switch
                {
                    StatusCodes.Status200OK => provider.Create200OkExample(),
                    StatusCodes.Status201Created => provider.Create201CreatedExample(),
                    StatusCodes.Status400BadRequest => provider.Create400BadRequestExample(context.Description.RelativePath),
                    StatusCodes.Status404NotFound => provider.Create404NotFoundExample(context.Description.RelativePath),
                    StatusCodes.Status409Conflict => provider.Create409ConflictExample(context.Description.RelativePath),
                    StatusCodes.Status500InternalServerError => provider.Create500InternalServerErrorExample(context.Description.RelativePath),
                    StatusCodes.Status503ServiceUnavailable => provider.Create503ServiceUnavailableExample(context.Description.RelativePath),
                    _ => null
                };

                if (responseExample is null || !response.Content.TryGetValue(responseExample.ContentType, out var mediaType))
                {
                    continue;
                }

                mediaType.Examples ??= new Dictionary<string, IOpenApiExample>();

                if (!mediaType.Examples.TryAdd(responseExample.Key, responseExample.Example))
                {
                    throw new InvalidOperationException(
                        $"Duplicated OpenAPI response example key '{responseExample.Key}' for status code '{statusCode}' and content type '{responseExample.ContentType}'."
                    );
                }
            }
        }

        return Task.CompletedTask;
    }
}