using Microsoft.OpenApi;
using System.Net.Http;
using WeatherMonitor.Api.OpenApi.Security;

namespace WeatherMonitor.Api.OpenApi.Transformers;

internal abstract class SecuritySchemeDocumentTransformer
{
    protected SecuritySchemeDocumentTransformer()
    {
    }

    protected virtual void ApplySecurity(OpenApiDocument document, OpenApiSecurityWrapper security)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[security.SchemeId] = security.Scheme;

        document.Security ??= [];

        var requirement = new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(security.SchemeId, document)] = [.. security.ScopeNames] };

        document.Security.Add(requirement);

        var operations = document.Paths.Select(path => path.Value)
            .Where(path => path.Operations is not null)
            .SelectMany(path => path.Operations!)
            .ToArray();

        foreach ((_, OpenApiOperation operation) in operations)
        {
            operation.Security ??= [];

            operation.Security.Add(requirement);
        }
    }
}