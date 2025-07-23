using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FractalDataWorks.Connections.Extensions;

/// <summary>
/// Extension methods for registering connection types with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all connection types found in the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly to scan for connection types. If null, uses the calling assembly.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConnectionTypes(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        
        var connectionTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => IsConnectionType(t))
            .ToList();

        foreach (var connectionType in connectionTypes)
        {
            // Register the concrete connection type
            services.TryAddSingleton(connectionType);
            
            // Register as IServiceFactory interfaces
            RegisterAsServiceFactory(services, connectionType);
        }

        return services;
    }

    /// <summary>
    /// Registers connection types from multiple assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">The assemblies to scan for connection types.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConnectionTypes(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            AddConnectionTypes(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers connection types from all loaded assemblies that reference FractalDataWorks.Connections.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConnectionTypesFromLoadedAssemblies(this IServiceCollection services)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic)
            .Where(a => a.GetReferencedAssemblies()
                .Any(ra => ra.Name?.StartsWith("FractalDataWorks.Connections") == true))
            .ToList();

        foreach (var assembly in assemblies)
        {
            AddConnectionTypes(services, assembly);
        }

        return services;
    }

    /// <summary>
    /// Registers a specific connection type.
    /// </summary>
    /// <typeparam name="TConnectionType">The connection type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConnectionType<TConnectionType>(this IServiceCollection services)
        where TConnectionType : class
    {
        if (!IsConnectionType(typeof(TConnectionType)))
        {
            throw new ArgumentException($"Type {typeof(TConnectionType).Name} is not a valid connection type", nameof(TConnectionType));
        }

        services.TryAddSingleton<TConnectionType>();
        RegisterAsServiceFactory(services, typeof(TConnectionType));

        return services;
    }

    private static bool IsConnectionType(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && 
                baseType.GetGenericTypeDefinition().Name.StartsWith("ConnectionTypeBase"))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static void RegisterAsServiceFactory(IServiceCollection services, Type connectionType)
    {
        var baseType = connectionType.BaseType;
        while (baseType != null && baseType.IsGenericType)
        {
            var genericDef = baseType.GetGenericTypeDefinition();
            if (genericDef.Name.StartsWith("ConnectionTypeBase"))
            {
                var genericArgs = baseType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    var connectionInterface = genericArgs[0]; // TConnection
                    var configType = genericArgs[1]; // TConfiguration
                    
                    // Register as IServiceFactory<TConnection, TConfiguration>
                    var factoryType = typeof(IServiceFactory<,>).MakeGenericType(connectionInterface, configType);
                    services.TryAddSingleton(factoryType, serviceProvider => serviceProvider.GetRequiredService(connectionType));
                    
                    // Register as IServiceFactory<TConnection>
                    var genericFactoryType = typeof(IServiceFactory<>).MakeGenericType(connectionInterface);
                    services.TryAddSingleton(genericFactoryType, serviceProvider => serviceProvider.GetRequiredService(connectionType));
                    
                    // Register as IServiceFactory
                    services.TryAddSingleton<IServiceFactory>(serviceProvider => (IServiceFactory)serviceProvider.GetRequiredService(connectionType));
                }
                break;
            }
            baseType = baseType.BaseType;
        }
    }
}