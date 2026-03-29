using Keycloak.AuthServices.Common;

namespace WeatherMonitor.Api.Infrastructure.Keycloak;

// ReSharper disable once ClassNeverInstantiated.Global
internal class KeycloakOAuth2Options : KeycloakInstallationOptions
{
    public string KeycloakAuthEndpoint => !string.IsNullOrWhiteSpace(KeycloakUrlRealm)
        ? $"{KeycloakUrlRealm}protocol/openid-connect/auth"
        : string.Empty;

    public bool RequireHttpsMetadata { get; set; } = true;

    [ConfigurationKeyName("Scopes")]
    public string[] Scopes { get; set; } = [];
}