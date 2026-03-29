using Microsoft.OpenApi;

namespace WeatherMonitor.Api.OpenApi.Security;

internal class OpenApiSecurityWrapper
{
    internal OpenApiSecurityWrapper(string schemeId, OpenApiSecurityScheme scheme, IReadOnlyList<string> scopeNames)
    {
        SchemeId = schemeId;
        Scheme = scheme;
        ScopeNames = scopeNames;
    }

    public string SchemeId { get; }

    public OpenApiSecurityScheme Scheme { get; }

    public IReadOnlyList<string> ScopeNames { get; }
}