using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using System.Text.Json;

namespace WeatherMonitor.Api.Infrastructure.Persistence.Extensions;

internal static class EntityTypeBuilderExtensions
{
    extension<TEntity>(EntityTypeBuilder<TEntity> builder) where TEntity : class
    {
        internal void ToTableSnakeCaseLower()
        {
            builder.ToTable(JsonNamingPolicy.SnakeCaseLower.ConvertName(typeof(TEntity).Name));
        }

        internal PropertyBuilder<TProperty> SnakeCaseLowerProperty<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            return propertyExpression.Body is MemberExpression memberExpression
                ? builder.Property(propertyExpression).HasColumnName(JsonNamingPolicy.SnakeCaseLower.ConvertName(memberExpression.Member.Name))
                : builder.Property(propertyExpression);
        }
    }

    extension<TComplex>(ComplexPropertyBuilder<TComplex> builder) where TComplex : notnull
    {
        internal ComplexTypePropertyBuilder<TProperty> SnakeCaseLowerProperty<TProperty>(Expression<Func<TComplex, TProperty>> propertyExpression)
        {
            return propertyExpression.Body is MemberExpression memberExpression
                ? builder.Property(propertyExpression).HasColumnName(JsonNamingPolicy.SnakeCaseLower.ConvertName($"{typeof(TComplex).Name}{memberExpression.Member.Name}"))
                : builder.Property(propertyExpression);
        }

        internal ComplexTypePropertyBuilder<TProperty> SnakeCaseLowerJsonProperty<TProperty>(Expression<Func<TComplex, TProperty>> propertyExpression)
        {
            return propertyExpression.Body is MemberExpression memberExpression
                ? builder.Property(propertyExpression).HasJsonPropertyName(JsonNamingPolicy.SnakeCaseLower.ConvertName(memberExpression.Member.Name))
                : builder.Property(propertyExpression);
        }

        internal ComplexPropertyBuilder<TComplex> HasSnakeCaseLowerJsonPropertyName()
        {
            return builder.HasJsonPropertyName(JsonNamingPolicy.SnakeCaseLower.ConvertName(builder.Metadata.Name));
        }
    }
}