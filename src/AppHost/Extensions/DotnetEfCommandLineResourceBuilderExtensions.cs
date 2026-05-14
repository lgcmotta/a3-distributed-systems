using AppHost.Helpers;

namespace AppHost.Extensions;

internal static class DotnetEfCommandLineResourceBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<ExecutableResource> AddDotNetEfDatabaseUpdateCommand(
            IResourceBuilder<PostgresDatabaseResource> database)
        {
            var workingDirectory = DirectoryHelper.FindRepositoryRoot();

            var cli = builder.AddExecutable(
                    name: "dotnet-ef",
                    command: "dotnet",
                    workingDirectory: workingDirectory,
                    args:
                    [
                        "ef",
                        "database",
                        "update",
                        "--project",
                        "src/WeatherMonitor.Api/WeatherMonitor.Api.csproj",
                        "--no-build",
                        "--verbose"
                    ])
                .WithReference(database)
                .WaitFor(database);

            return cli;
        }
    }
}