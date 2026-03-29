using Microsoft.OpenApi;
using System.Diagnostics.CodeAnalysis;

namespace WeatherMonitor.Api.OpenApi.Security;

internal class OpenApiOAuth2SecurityBuilder
{
    private const string DefaultOpenApiDescription = "OAuth2 Authentication";
    private const string DefaultOpenApiReferenceId = "OAuth2";
    private string _schemeId = DefaultOpenApiReferenceId;

    private readonly Dictionary<string, string> _scopes = [];
    private readonly OpenApiSecurityScheme _scheme = new()
    {
        Type = SecuritySchemeType.OAuth2,
        Description = DefaultOpenApiDescription,
        Flows = new OpenApiOAuthFlows
        {
            ClientCredentials = new OpenApiOAuthFlow(),
            AuthorizationCode = new OpenApiOAuthFlow()
        },
    };

    public OpenApiOAuth2SecurityBuilder WithSchemeDescription(string description = DefaultOpenApiDescription)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return this;
        }

        _scheme.Description = description;

        return this;
    }

    public OpenApiOAuth2SecurityBuilder WithAuthorizationUrl([StringSyntax("Uri", "uriKind")] string authorizationUrl)
    {
        return Uri.IsWellFormedUriString(authorizationUrl, UriKind.Absolute)
            ? WithAuthorizationUrl(new Uri(authorizationUrl))
            : throw new UriFormatException("Authorization URL must be a valid absolute URL");
    }

    private OpenApiOAuth2SecurityBuilder WithAuthorizationUrl(Uri authorizationUrl)
    {
        _scheme.Flows?.AuthorizationCode?.AuthorizationUrl = authorizationUrl;

        _scheme.Flows?.ClientCredentials?.AuthorizationUrl = authorizationUrl;

        return this;
    }

    public OpenApiOAuth2SecurityBuilder WithTokenUrl([StringSyntax("Uri", "uriKind")] string tokenUrl)
    {
        return Uri.IsWellFormedUriString(tokenUrl, UriKind.Absolute)
            ? WithTokenUrl(new Uri(tokenUrl))
            : throw new UriFormatException("Token URL must be a valid absolute URL");
    }

    private OpenApiOAuth2SecurityBuilder WithTokenUrl(Uri tokenUrl)
    {
        _scheme.Flows?.AuthorizationCode?.TokenUrl = tokenUrl;

        _scheme.Flows?.ClientCredentials?.TokenUrl = tokenUrl;

        return this;
    }

    public OpenApiOAuth2SecurityBuilder WithOpenIdScope(string? scopeDescription = null)
    {
        var description = !string.IsNullOrWhiteSpace(scopeDescription)
            ? scopeDescription
            : "Grants basic OpenID claims";

        _scopes.TryAdd("openid", description);

        return this;
    }

    public OpenApiOAuth2SecurityBuilder WithProfileScope(string? scopeDescription = null)
    {
        var description = !string.IsNullOrWhiteSpace(scopeDescription)
            ? scopeDescription
            : "Grants access to basic user profile information";

        _scopes.TryAdd("profile", description);

        return this;
    }

    public OpenApiOAuth2SecurityBuilder WithEmailScope(string? scopeDescription = null)
    {
        var description = !string.IsNullOrWhiteSpace(scopeDescription)
            ? scopeDescription
            : "Grants access to the user’s email address";

        _scopes.TryAdd("email", description);

        return this;
    }

    public OpenApiOAuth2SecurityBuilder WithCustomScope(string scope, string scopeDescription)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeDescription);

        _scopes.TryAdd(scope, scopeDescription);

        return this;
    }

    public OpenApiOAuth2SecurityBuilder WithSchemeReferenceId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        _schemeId = id;

        return this;
    }

    internal OpenApiSecurityWrapper Build()
    {
        _scheme.Flows ??= new OpenApiOAuthFlows();
        _scheme.Flows.ClientCredentials ??= new OpenApiOAuthFlow();
        _scheme.Flows.AuthorizationCode ??= new OpenApiOAuthFlow();

        if (_scheme.Flows.ClientCredentials.TokenUrl is null)
        {
            throw new InvalidOperationException("The token URL is required to use OAuth2 client credentials flow.");
        }

        if (_scheme.Flows.ClientCredentials.TokenUrl is null)
        {
            throw new InvalidOperationException("The token URL is required to use OAuth2 authorization code flow.");
        }

        _scheme.Flows.ClientCredentials.Scopes = new Dictionary<string, string>(_scopes);
        _scheme.Flows.AuthorizationCode.Scopes = new Dictionary<string, string>(_scopes);

        var scopeNames = _scopes.Keys.ToArray();

        return new OpenApiSecurityWrapper(_schemeId, _scheme, scopeNames);
    }
}