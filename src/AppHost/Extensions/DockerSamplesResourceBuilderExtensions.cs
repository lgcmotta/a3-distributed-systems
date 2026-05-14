using AppHost.Helpers;

namespace AppHost.Extensions;

internal static class DockerSamplesResourceBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<ContainerResource> AddSampleApiNodeJS()
        {
            var rootDirectory = DirectoryHelper.FindRepositoryRoot();

            var contextPath = Path.Join(rootDirectory, "samples", "nodejs");

            return builder.AddDockerfile(name: "Sample-NodeJS", contextPath, dockerfilePath: "Dockerfile")
                .WithContainerName(name: "samples-nodejs")
                .WithImage(image: "weather-monitor/samples/nodejs", tag: "latest")
                .WithLifetime(lifetime: ContainerLifetime.Persistent)
                .WithHttpEndpoint(
                    targetPort: 3000,
                    name: "http",
                    env: "PORT")
                .WithHttpHealthCheck(
                    path: "/healthz",
                    statusCode: 200,
                    endpointName: "http");
        }

        internal IResourceBuilder<ContainerResource> AddSampleApiGo()
        {
            var rootDirectory = DirectoryHelper.FindRepositoryRoot();

            var contextPath = Path.Join(rootDirectory, "samples", "go");

            return builder.AddDockerfile(name: "Sample-Go", contextPath, dockerfilePath: "Dockerfile")
                .WithContainerName(name: "samples-go")
                .WithImage(image: "weather-monitor/samples/go", tag: "latest")
                .WithLifetime(lifetime: ContainerLifetime.Persistent)
                .WithHttpEndpoint(
                    targetPort: 8080,
                    name: "http",
                    env: "PORT")
                .WithHttpHealthCheck(
                    path: "/healthz",
                    statusCode: 200,
                    endpointName: "http");
        }

        internal IResourceBuilder<ContainerResource> AddSampleApiPython()
        {
            var rootDirectory = DirectoryHelper.FindRepositoryRoot();

            var contextPath = Path.Join(rootDirectory, "samples", "python");

            return builder.AddDockerfile(name: "Sample-Python", contextPath, dockerfilePath: "Dockerfile")
                .WithContainerName(name: "samples-python")
                .WithImage(image: "weather-monitor/samples/python", tag: "latest")
                .WithLifetime(lifetime: ContainerLifetime.Persistent)
                .WithHttpEndpoint(
                    targetPort: 5000,
                    name: "http",
                    env: "PORT")
                .WithHttpHealthCheck(
                    path: "/healthz",
                    statusCode: 200,
                    endpointName: "http");
        }
    }
}