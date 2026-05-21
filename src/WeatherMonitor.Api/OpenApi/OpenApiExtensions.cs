using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using WeatherMonitor.Api.OpenApi.Providers;
using WeatherMonitor.Api.OpenApi.Security;
using WeatherMonitor.Api.OpenApi.Transformers;

namespace WeatherMonitor.Api.OpenApi;

internal static class OpenApiExtensions
{
    extension(OpenApiOptions options)
    {
        internal OpenApiOptions AddOAuth2SecurityScheme(Action<OpenApiOAuth2SecurityBuilder> configureOAuth2)
        {
            return options.AddDocumentTransformer(new OAuth2SecurityTransformer(configureOAuth2));
        }

        internal OpenApiOptions AddExamplesTransformer()
        {
            return options.AddOperationTransformer(new OpenApiExampleTransformer());
        }

        internal OpenApiOptions WithOpenApiInfo(Action<OpenApiInfo> configureInfo)
        {
            var info = new OpenApiInfo();

            configureInfo(info);

            return options.AddDocumentTransformer(new OpenApiInfoTransformer(info));
        }
    }

    extension(RouteHandlerBuilder builder)
    {
        internal RouteHandlerBuilder WithOpenApiExampleProvider<TProvider>() where TProvider : class, IOpenApiEndpointExampleProvider, new()
        {
            return builder.WithMetadata(new TProvider());
        }
    }
}