namespace AppHost.Extensions;

internal static class KeycloakResourceBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<KeycloakResource> AddKeycloakContainer(IResourceBuilder<PostgresDatabaseResource> database)
        {
            return builder.AddKeycloak(name: "Keycloak", port: 8080)
                .WithImageRegistry(registry: "quay.io")
                .WithImage(image: "keycloak/keycloak", tag: "26.5")
                .WithContainerName("keycloak")
                .WithLifetime(lifetime: ContainerLifetime.Persistent)
                .WithRealmImport(import: "./Realms/realm.json")
                .WithDataVolume("keycloak_data")
                .WithReference(source: database)
                .WithOtlpExporter()
                .WaitFor(dependency: database);
        }
    }
}