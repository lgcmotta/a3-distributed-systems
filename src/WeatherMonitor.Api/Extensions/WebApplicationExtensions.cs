using Keycloak.AuthServices.Common;
using Scalar.AspNetCore;
using WeatherMonitor.Api.Infrastructure.Keycloak;

namespace WeatherMonitor.Api.Extensions;

internal static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        internal WebApplication UsePermissiveCors()
        {
            app.UseCors("Permissive");

            return app;
        }

        internal WebApplication MapOpenApiUI()
        {
            var keycloak = app.Configuration.GetKeycloakOptions<KeycloakOAuth2Options>();

            app.MapScalarApiReference(options =>
            {
                options.WithTheme(ScalarTheme.DeepSpace)
                    .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                    .WithOpenApiRoutePattern("/openapi/{documentName}.json")
                    .WithTitle("Weather Monitor API");

                if (keycloak is null)
                {
                    return;
                }

                options.AddClientCredentialsFlow("OAuth2", flow =>
                {
                    flow.WithTokenUrl(keycloak.KeycloakTokenEndpoint);
                    flow.WithSelectedScopes(keycloak.Scopes);
                });

                options.AddAuthorizationCodeFlow("OAuth2", flow =>
                {
                    flow.WithAuthorizationUrl(keycloak.KeycloakAuthEndpoint);
                    flow.WithTokenUrl(keycloak.KeycloakTokenEndpoint);
                    flow.WithPkce(Pkce.Sha256);
                    flow.WithSelectedScopes(keycloak.Scopes);
                });
            });

            return app;
        }

        internal RouteGroupBuilder MapApiGroup()
        {
            return app.NewVersionedApi("Weather Monitor API")
                .MapGroup("/api/v{version:apiVersion}");
        }
    }
}