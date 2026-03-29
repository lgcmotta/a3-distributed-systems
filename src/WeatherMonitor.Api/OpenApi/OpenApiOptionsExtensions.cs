using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using WeatherMonitor.Api.OpenApi.Security;
using WeatherMonitor.Api.OpenApi.Transformers;

namespace WeatherMonitor.Api.OpenApi;

internal static class OpenApiOptionsExtensions
{
    extension(OpenApiOptions options)
    {
        internal OpenApiOptions AddOAuth2SecurityScheme(Action<OpenApiOAuth2SecurityBuilder> configureOAuth2)
        {
            return options.AddDocumentTransformer(new OAuth2SecurityTransformer(configureOAuth2));
        }

        internal OpenApiOptions WithOpenApiInfo(Action<OpenApiInfo> configureInfo)
        {
            var info = new OpenApiInfo();

            configureInfo(info);

            return options.AddDocumentTransformer(new OpenApiInfoTransformer(info));
        }
    }
}