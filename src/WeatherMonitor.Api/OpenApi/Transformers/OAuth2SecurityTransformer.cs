using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using WeatherMonitor.Api.OpenApi.Security;

namespace WeatherMonitor.Api.OpenApi.Transformers;

internal sealed class OAuth2SecurityTransformer : SecuritySchemeDocumentTransformer, IOpenApiDocumentTransformer
{
    private readonly Action<OpenApiOAuth2SecurityBuilder> _configureOAuth2;

    internal OAuth2SecurityTransformer(Action<OpenApiOAuth2SecurityBuilder> configureOAuth2)
    {
        _configureOAuth2 = configureOAuth2;
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken = default)
    {
        var provider = context.ApplicationServices.GetService<IAuthenticationSchemeProvider>();

        if (provider is null)
        {
            return;
        }

        var schemes = await provider.GetAllSchemesAsync();

        if (schemes.All(scheme => scheme.Name != JwtBearerDefaults.AuthenticationScheme))
        {
            return;
        }

        var builder = new OpenApiOAuth2SecurityBuilder();

        _configureOAuth2(builder);

        var security = builder.Build();

        ApplySecurity(document, security);
    }
}