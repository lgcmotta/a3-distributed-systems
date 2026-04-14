namespace WeatherMonitor.Api.Infrastructure.Extensions;

internal static class TypeExtensions
{
    extension(Type type)
    {
        internal string GetGenericTypeName()
        {
            if (type.IsArray)
            {
                return $"{type.GetElementType()!.GetGenericTypeName()}[]";
            }

            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var tickIndex = type.Name.IndexOf('`');

            var typeName = tickIndex >= 0 ? type.Name[..tickIndex] : type.Name;

            var genericArguments = string.Join(", ", type.GetGenericArguments().Select(x => x.GetGenericTypeName()));

            return $"{typeName}<{genericArguments}>";
        }
    }
}