using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WeatherMonitor.Api.OpenApi.Transformers;

internal sealed class OpenApiInfoTransformer : IOpenApiDocumentTransformer
{
    private readonly OpenApiInfo _info;

    internal OpenApiInfoTransformer(OpenApiInfo info)
    {
        _info = info;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken = default)
    {
        document.Info = _info;

        return Task.CompletedTask;
    }
}