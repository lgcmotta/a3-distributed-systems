using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using WeatherMonitor.Api.Infrastructure.Persistence.Extensions;
using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Deliveries;
using WeatherMonitor.Domain.Deliveries.ValueObjects;

namespace WeatherMonitor.Api.Infrastructure.Persistence.Mappings;

public class WebhookDeliveryEntityTypeConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTableSnakeCaseLower();

        builder.SnakeCaseLowerProperty(delivery => delivery.Id)
            .ValueGeneratedOnAdd()
            .HasValueGenerator<GuidValueGenerator>()
            .IsRequired();

        builder.SnakeCaseLowerProperty(delivery => delivery.ScheduledFor)
            .IsRequired();

        builder.SnakeCaseLowerProperty(delivery => delivery.DeliveredAt)
            .IsRequired(false);

        builder.ComplexProperty(delivery => delivery.Payload, complex =>
        {
            complex.ToJson("payload");

            complex.SnakeCaseLowerJsonProperty(payload => payload.MonitorId);
            complex.SnakeCaseLowerJsonProperty(payload => payload.ClientId);
            complex.SnakeCaseLowerJsonProperty(payload => payload.ForecastDate);

            complex.ComplexProperty(property => property.Location, property =>
            {
                property.SnakeCaseLowerJsonProperty(location => location.Code);
                property.SnakeCaseLowerJsonProperty(location => location.Name);
                property.SnakeCaseLowerJsonProperty(location => location.State);
            });

            complex.ComplexProperty(property => property.WeatherCondition, property =>
            {
                property.SnakeCaseLowerJsonProperty(condition => condition.Code);
                property.SnakeCaseLowerJsonProperty(condition => condition.Description);
            });
        });

        builder.SnakeCaseLowerProperty(delivery => delivery.Status)
            .HasConversion(status => status.Key, key => Enumeration.ParseByKey<WebhookDeliveryStatus>(key))
            .IsRequired();

        builder.SnakeCaseLowerProperty(delivery => delivery.RetryCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.SnakeCaseLowerProperty(delivery => delivery.JobId)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.SnakeCaseLowerProperty(delivery => delivery.FailureReason)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property<DateTimeOffset>("created_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property<DateTimeOffset?>("updated_at")
            .IsRequired(false);

        builder.HasIndex(delivery => new { delivery.Status, delivery.ScheduledFor })
            .HasDatabaseName("ix_webhook_delivery_status_scheduled_for");
    }
}