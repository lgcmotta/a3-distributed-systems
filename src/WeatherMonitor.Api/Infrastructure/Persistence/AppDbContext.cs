using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.CommandLine;
using TickerQ.EntityFrameworkCore.Configurations;
using TickerQ.Utilities.Entities;
using WeatherMonitor.Domain.Deliveries;
using WeatherMonitor.Domain.Monitors;

namespace WeatherMonitor.Api.Infrastructure.Persistence;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WeatherMonitorConfiguration> Monitors => Set<WeatherMonitorConfiguration>();
    public DbSet<WebhookDelivery> Deliveries => Set<WebhookDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.ApplyConfiguration(new TimeTickerConfigurations<TimeTickerEntity>());
        modelBuilder.ApplyConfiguration(new CronTickerConfigurations<CronTickerEntity>());
        modelBuilder.ApplyConfiguration(new CronTickerOccurrenceConfigurations<CronTickerEntity>());
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