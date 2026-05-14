using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace WeatherMonitor.Domain.Core;

public abstract record Enumeration
{
    private const BindingFlags BindingAttributes = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

    public int Key { get; }

    public string Value { get; }

    protected Enumeration(int key, string value)
    {
        Key = key;
        Value = value;
    }

    public static IEnumerable<TEnumeration> Enumerate<TEnumeration>()
    {
        var type = typeof(TEnumeration);

        var fields = type.GetFields(bindingAttr: BindingAttributes);
        var properties = type.GetProperties(bindingAttr: BindingAttributes);

        return properties.Select(property => property.GetValue(null))
            .Union(fields.Select(field => field.GetValue(null)))
            .Cast<TEnumeration>();
    }

    public static TEnumeration ParseByKey<TEnumeration>(int key) where TEnumeration : Enumeration
    {
        return Convert<TEnumeration, int>(key, nameof(Key), e => e.Key == key);
    }


    public static TEnumeration ParseByValue<TEnumeration>(string value) where TEnumeration : Enumeration
    {
        return Convert<TEnumeration, string>(value, nameof(Value), e => e.Value == value);
    }

    public static bool TryParseByKey<TEnumeration>(int key, [NotNullWhen(returnValue: true)] out TEnumeration? enumeration) where TEnumeration : Enumeration
    {
        return TryConvert(e => e.Key == key, out enumeration);
    }

    public static bool TryParseByValue<TEnumeration>(string value, [NotNullWhen(returnValue: true)] out TEnumeration? enumeration) where TEnumeration : Enumeration
    {
        return TryConvert(e => e.Value == value, out enumeration);
    }

    private static TEnumeration Convert<TEnumeration, TValue>(TValue value, string name, Func<TEnumeration, bool> predicate)
        where TEnumeration : Enumeration
    {
        _ = TryConvert(predicate, out var enumeration);

        return enumeration ?? throw new ArgumentOutOfRangeException(nameof(value), $"{value} is not a valid {name} for type {typeof(TEnumeration)}");
    }

    private static bool TryConvert<TEnumeration>(Func<TEnumeration, bool> predicate, [NotNullWhen(returnValue: true)] out TEnumeration? enumeration)
        where TEnumeration : Enumeration
    {
        enumeration = Enumerate<TEnumeration>().FirstOrDefault(predicate);
        return enumeration is not null;
    }

    public override string ToString() => Value;
}