using WeatherMonitor.Domain.Core;

namespace WeatherMonitor.Api.Infrastructure.Persistence.Extensions;

internal static class DbContextExtensions
{
    extension(AppDbContext context)
    {
        internal DateTimeOffset ReadCreatedAtShadowProperty<TAggregate>(TAggregate aggregate)
            where TAggregate : class, IAggregateRoot
        {
            return context.Entry(aggregate).Property<DateTimeOffset>("created_at").CurrentValue;
        }

        internal DateTimeOffset? ReadUpdatedAtShadowProperty<TAggregate>(TAggregate aggregate)
            where TAggregate : class, IAggregateRoot
        {
            return context.Entry(aggregate).Property<DateTimeOffset?>("updated_at").CurrentValue;
        }
    }
}