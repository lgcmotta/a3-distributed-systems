using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using WeatherMonitor.Api.Infrastructure.Persistence.Extensions;
using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Monitors;
using WeatherMonitor.Domain.Monitors.ValueObjects;
using WeatherCondition = WeatherMonitor.Domain.Monitors.ValueObjects.WeatherCondition;

namespace WeatherMonitor.Api.Infrastructure.Persistence.Mappings;

internal sealed class WeatherMonitorEntityTypeConfiguration : IEntityTypeConfiguration<WeatherMonitorConfiguration>
{
    public void Configure(EntityTypeBuilder<WeatherMonitorConfiguration> builder)
    {
        builder.ToTableSnakeCaseLower();

        builder.SnakeCaseLowerProperty(monitor => monitor.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<GuidValueGenerator>()
            .IsRequired();

        builder.HasKey(monitor => monitor.Id);

        builder.SnakeCaseLowerProperty(monitor => monitor.ClientId)
            .IsRequired()
            .HasMaxLength(200);

        builder.ComplexProperty(monitor => monitor.Location, complex =>
        {
            complex.SnakeCaseLowerProperty(location => location.CityCode)
                .IsRequired();

            complex.SnakeCaseLowerProperty(location => location.CityName)
                .HasMaxLength(100)
                .IsRequired();

            complex.SnakeCaseLowerProperty(location => location.State)
                .HasMaxLength(2)
                .HasConversion(state => state.Value, value => Enumeration.ParseByValue<BrazilianState>(value))
                .IsRequired();
        });

        builder.SnakeCaseLowerProperty(monitor => monitor.WeatherCondition)
            .HasConversion(condition => condition.Code, code => WeatherCondition.FromCode(code))
            .IsRequired();

        builder.ComplexProperty(monitor => monitor.Webhook, complex =>
        {
            complex.SnakeCaseLowerProperty(webhook => webhook.Url)
                .HasMaxLength(500)
                .IsRequired();

            complex.SnakeCaseLowerProperty(webhook => webhook.AccessToken)
                .HasMaxLength(500)
                .IsRequired(false);

            complex.SnakeCaseLowerProperty(webhook => webhook.ScheduleFor)
                .IsRequired();

            complex.SnakeCaseLowerProperty(webhook => webhook.TimeZoneId)
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.SnakeCaseLowerProperty(monitor => monitor.Enabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(monitor => monitor.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now()")
            .ValueGeneratedOnAdd()
            .IsRequired();

        builder.Property<DateTimeOffset?>("updated_at")
            .IsRequired(false);
    }
}