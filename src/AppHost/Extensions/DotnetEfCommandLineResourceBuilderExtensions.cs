namespace AppHost.Extensions;

internal static class DotnetEfCommandLineResourceBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        internal IResourceBuilder<ExecutableResource> AddDotNetEfDatabaseUpdateCommand(
            IResourceBuilder<PostgresDatabaseResource> database)
        {
            var workingDirectory = FindRepositoryRoot();

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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var solutionFile = Path.Combine(directory.FullName, "WeatherMonitor.slnx");

            if (File.Exists(solutionFile))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}