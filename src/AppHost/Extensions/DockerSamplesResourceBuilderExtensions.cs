using AppHost.Helpers;

namespace AppHost.Extensions;

internal static class DockerSamplesResourceBuilderExtensions
{
    private sealed record SampleApp(string Name, string Language, int Port);

    private static readonly IReadOnlyList<SampleApp> SampleApps =
    [
        new(Name: "Sample-NodeJS", Language: "nodejs", Port: 3000),
        new(Name: "Sample-Go", Language: "go", Port: 8080),
        new(Name: "Sample-Python", Language: "python", Port: 5000)
    ];

    extension(IDistributedApplicationBuilder builder)
    {
        internal void AddSampleApps()
        {
            var rootDirectory = DirectoryHelper.FindRepositoryRoot();

            foreach (var app in SampleApps)
            {
                var contextPath = Path.Join(rootDirectory, "samples", app.Language);

                builder.AddDockerfile(name: app.Name, contextPath, dockerfilePath: "Dockerfile")
                    .WithContainerName(name: $"samples-{app.Language}")
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
            }
        }
    }
}