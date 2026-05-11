using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;

namespace WeatherMonitor.Api.Infrastructure.Persistence.Interceptors;

internal static class InterceptorAssemblyScanner
{
    internal static IInterceptor[] Scan(IServiceProvider? serviceProvider = null, params Assembly[] interceptorAssemblies)
    {
        if (interceptorAssemblies is { Length: 0 })
        {
            return [];
        }

        var interceptorTypes = interceptorAssemblies
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsClass: true, IsAbstract: false } &&
                           type.IsAssignableTo(typeof(IInterceptor)));


        if (serviceProvider is not null)
        {
            return interceptorTypes
                .Select(type => ActivatorUtilities.CreateInstance(serviceProvider, type))
                .Cast<IInterceptor>()
                .ToArray();
        }

        return interceptorTypes
            .Where(type => type.GetConstructor(Type.EmptyTypes) is not null)
            .Select(Activator.CreateInstance)
            .Cast<IInterceptor>()
            .ToArray();
    }
}