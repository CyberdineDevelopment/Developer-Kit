using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FractalDataWorks.Services.Extensions;

/// <summary>
/// Extension methods for registering service types with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all service types found in the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for service types. If null, uses the calling assembly.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServiceTypes(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var serviceTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => IsServiceType(t))
            .ToList();

        foreach (var serviceType in serviceTypes)
        {
            // Register the concrete service type
            services.TryAddSingleton(serviceType);

            // Register as IServiceFactory interfaces
            RegisterAsServiceFactory(services, serviceType);
        }

        return services;
    }

    /// <summary>
    /// Registers service types from multiple assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for service types.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServiceTypes(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            AddServiceTypes(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers service types from all loaded assemblies that reference FractalDataWorks.Services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServiceTypesFromLoadedAssemblies(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Where(a => a.GetReferencedAssemblies()
                .Any(ra => ra.Name?.StartsWith("FractalDataWorks.Services", StringComparison.Ordinal) == true))
            .ToList();

        foreach (var assembly in assemblies)
        {
            AddServiceTypes(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers a specific service type.
    /// </summary>
    /// <typeparam name="TServiceType">The service type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
#pragma warning disable MA0015 // TServiceType is an accurate and descriptive name for this context
    public static IServiceCollection AddServiceType<TServiceType>(this IServiceCollection services)
        where TServiceType : class
    {
        if (!IsServiceType(typeof(TServiceType)))
        {
            throw new ArgumentException($"Type {typeof(TServiceType).Name} is not a valid service type", nameof(TServiceType));
        }
#pragma warning restore MA0015

        services.TryAddSingleton<TServiceType>();
        RegisterAsServiceFactory(services, typeof(TServiceType));

        return services;
    }

    private static bool IsServiceType(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition().Name.StartsWith("ServiceTypeBase", StringComparison.Ordinal))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static void RegisterAsServiceFactory(IServiceCollection services, Type serviceType)
    {
        var baseType = serviceType.BaseType;
        while (baseType != null && baseType.IsGenericType)
        {
            var genericDef = baseType.GetGenericTypeDefinition();
            if (genericDef.Name.StartsWith("ServiceTypeBase", StringComparison.Ordinal))
            {
                var genericArgs = baseType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    var serviceInterface = genericArgs[0]; // TService
                    var configType = genericArgs[1]; // TConfiguration

                    // Register as IServiceFactory<TService, TConfiguration>
                    var factoryType = typeof(IServiceFactory<,>).MakeGenericType(serviceInterface, configType);
                    services.TryAddSingleton(factoryType, serviceProvider => serviceProvider.GetRequiredService(serviceType));

                    // Register as IServiceFactory<TService>
                    var genericFactoryType = typeof(IServiceFactory<>).MakeGenericType(serviceInterface);
                    services.TryAddSingleton(genericFactoryType, serviceProvider => serviceProvider.GetRequiredService(serviceType));

                    // Register as IServiceFactory
                    services.TryAddSingleton<IServiceFactory>(serviceProvider => (IServiceFactory)serviceProvider.GetRequiredService(serviceType));
                }
                break;
            }
            baseType = baseType.BaseType;
        }
    }
}