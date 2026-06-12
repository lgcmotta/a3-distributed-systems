using AppHost.Helpers;

namespace AppHost.Extensions;

internal static class DockerSamplesResourceBuilderExtensions
{
    private sealed record SampleApp(string Language, int Port);

    private static readonly IReadOnlyList<SampleApp> SampleApps =
    [
        new(Language: "nodejs", Port: 3000),
        new(Language: "go", Port: 8081),
        new(Language: "python", Port: 5000)
    ];

    extension(IDistributedApplicationBuilder builder)
    {
        internal IReadOnlyCollection<IResourceBuilder<ContainerResource>> AddSampleApps()
        {
            var rootDirectory = DirectoryHelper.FindRepositoryRoot();

            IReadOnlyList<IResourceBuilder<ContainerResource>> resources =
            [
                ..SampleApps
                    .Select(app =>
                    {
                        var contextPath = Path.Join(rootDirectory, "samples", app.Language);

                        var name = $"samples-{app.Language}";

                        return builder.AddDockerfile(name: name, contextPath, dockerfilePath: "Dockerfile")
                            .WithContainerName(name: name)
                            .WithImage(image: $"weather-monitor/samples/{app.Language}", tag: "latest")
                            .WithLifetime(lifetime: ContainerLifetime.Persistent)
                            .WithHttpEndpoint(
                                targetPort: app.Port,
                                name: "http",
                                env: "PORT")
                            .WithHttpHealthCheck(
                                path: "/healthz",
                                statusCode: 200,
                                endpointName: "http"
                            );
                    })
            ];

            return resources;
        }
    }
}