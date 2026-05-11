using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WeatherMonitor.Domain.Core;

namespace WeatherMonitor.Api.Infrastructure.Persistence.Interceptors;

internal sealed class AuditableEntitySaveChangesInterceptor(TimeProvider provider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SaveChangesOnAuditableProperties(eventData);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SaveChangesOnAuditableProperties(eventData);

        return base.SavingChanges(eventData, result);
    }

    private void SaveChangesOnAuditableProperties(DbContextEventData eventData)
    {
        var entries = eventData.Context?.ChangeTracker.Entries<IAggregateRoot>().ToArray() ?? [];

        foreach (var entry in entries)
        {
            if (entry is { State: EntityState.Added })
            {
                entry.Property<DateTimeOffset>("created_at").CurrentValue = provider.GetUtcNow();

                if (entry.Properties.ContainsShadowProperty("updated_at"))
                {
                    entry.Property<DateTimeOffset?>("updated_at").CurrentValue = null;
                }
            }

            if (entry is { State: EntityState.Modified })
            {
                entry.Property<DateTimeOffset?>("updated_at").CurrentValue = provider.GetUtcNow();
            }
        }
    }
}

file static class PropertyEntryExtensions
{
    extension(IEnumerable<PropertyEntry> entries)
    {
        internal bool ContainsShadowProperty(string name)
        {
            return entries.Any(entry => entry.IsShadowProperty(name));
        }
    }

    extension(PropertyEntry entry)
    {
        private bool IsShadowProperty(string name)
        {
            return entry.Metadata.IsShadowProperty() && entry.Metadata.Name == name;
        }
    }
}