using FluentValidation;
using WeatherMonitor.Domain.Core;
using WeatherMonitor.Domain.Monitors.ValueObjects;

namespace WeatherMonitor.Api.Extensions;

internal static class AbstractValidatorExtensions
{
    extension<T>(IRuleBuilder<T, string> builder)
    {
        internal IRuleBuilderOptions<T, string> BeBrazilianStateAcronym()
        {
            return builder.BeConvertibleToEnumeration<T, string, BrazilianState>();
        }
    }

    extension<T, TProperty>(IRuleBuilder<T, TProperty> builder)
    {
        private IRuleBuilderOptions<T, TProperty> BeConvertibleToEnumeration<TEnumeration>()
            where TEnumeration : Enumeration
        {
            return builder.Must(property => property switch
            {
                string value => Enumeration.Enumerate<TEnumeration>()
                    .Any(enumeration => enumeration.Value.Equals(value.Trim(), StringComparison.InvariantCultureIgnoreCase)),
                int key => Enumeration.Enumerate<TEnumeration>().Any(enumeration => enumeration.Key == key),
                _ => false
            });
        }
    }
}