using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.CommandLine;

namespace WeatherMonitor.Api.Infrastructure.Persistence;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

internal sealed class AppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var option = new Option<string>("--connection");

        var root = new RootCommand { option };

        string? connectionString = string.Empty;

        root.SetAction(value => connectionString = value.GetValue(option) ?? Environment.GetEnvironmentVariable("ConnectionStrings__WeatherMonitorDB"));

        var result = root.Parse(args);

        result.Invoke();

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("""
                                                PostgreSQL connection string cannot be empty or white-space. 
                                                Please, specify the connection string either using '--connection' or 
                                                by setting the environment variable 'ConnectionStrings__WeatherMonitorDB'
                                                """);
        }

        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString);

        return new AppDbContext(builder.Options);
    }
}