using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace VibedMediatr;

/// <summary>
/// Extension methods for registering VibedMediatr with the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds VibedMediatr to the service collection, scanning the entry assembly only.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVibedMediatr(this IServiceCollection services)
        => AddVibedMediatr(services, Array.Empty<Assembly>());

    /// <summary>
    /// Adds VibedMediatr to the service collection, scanning the entry assembly and any additional assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="additionalAssemblies">Additional assemblies to scan for handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVibedMediatr(this IServiceCollection services, params Assembly[] additionalAssemblies)
    {
        var assemblies = new HashSet<Assembly>();
        var entry = Assembly.GetEntryAssembly();
        if (entry != null) assemblies.Add(entry);
        foreach (var assembly in additionalAssemblies.Where(a => a != null)) assemblies.Add(assembly);

        RegisterHandlers(services, assemblies);
        services.AddScoped<IMediator, Mediator>();
        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types!.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (type is null || type.IsAbstract || type.IsInterface) continue;

                foreach (var iface in type.GetInterfaces())
                {
                    if (!iface.IsGenericType) continue;
                    var genericTypeDef = iface.GetGenericTypeDefinition();
                    if (genericTypeDef == typeof(IRequestHandler<,>) || genericTypeDef == typeof(IRequestHandler<>))
                    {
                        services.AddTransient(iface, type);
                    }
                }
            }
        }
    }
}
