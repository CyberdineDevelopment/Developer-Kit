using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FractalDataWorks.Services;

namespace FractalDataWorks.Tools.Extensions;

/// <summary>
/// Extension methods for registering tool types with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all tool types from the specified assembly with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to add the tool types to.</param>
    /// <param name="assembly">The assembly to scan for tool types. If null, uses the calling assembly.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddToolTypes(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();
        
        var toolTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => IsToolType(t))
            .ToList();

        foreach (var toolType in toolTypes)
        {
            services.TryAddSingleton(toolType);
            RegisterAsToolFactory(services, toolType);
        }

        return services;
    }
    
    private static bool IsToolType(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && 
                baseType.GetGenericTypeDefinition().Name.StartsWith("ToolTypeBase"))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static void RegisterAsToolFactory(IServiceCollection services, Type toolType)
    {
        var baseType = toolType.BaseType;
        while (baseType != null && baseType.IsGenericType)
        {
            var genericDef = baseType.GetGenericTypeDefinition();
            if (genericDef.Name.StartsWith("ToolTypeBase"))
            {
                var genericArgs = baseType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    var toolInterface = genericArgs[0]; // TTool
                    var configType = genericArgs[1]; // TConfiguration
                    
                    var factoryType = typeof(IToolFactory<,>).MakeGenericType(toolInterface, configType);
                    services.TryAddSingleton(factoryType, serviceProvider => serviceProvider.GetRequiredService(toolType));
                }
                break;
            }
            baseType = baseType.BaseType;
        }
    }
}