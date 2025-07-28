using System;
using System.Linq;
using FractalDataWorks.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections.Data;

/// <summary>
/// Extension methods for registering data providers
/// </summary>
public static class DataProviderRegistration
{
    /// <summary>
    /// Adds data provider services to the DI container
    /// </summary>
    public static IServiceCollection AddDataProviders(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register configuration
        var providerConfig = new DataProviderConfiguration();
        configuration.GetSection("DataProvider").Bind(providerConfig);
        services.AddSingleton(providerConfig);
        
        // TODO: Auto-register all discovered providers when enhanced enum is available
        if (providerConfig.AutoRegisterProviders)
        {
            // Manual registration for now
            // RegisterProvider(services, new SqlServerProviderType());
        }
        
        // Register the service provider
        services.AddScoped<IDataServiceProvider, DataServiceProvider>();
        
        return services;
    }
    
    /// <summary>
    /// Adds a specific data provider to the DI container
    /// </summary>
    public static IServiceCollection AddDataProvider<TProvider>(this IServiceCollection services)
        where TProvider : IDataProviderType, new()
    {
        var provider = new TProvider();
        RegisterProvider(services, provider);
        return services;
    }
    
    private static void RegisterProvider(IServiceCollection services, IDataProviderType providerType)
    {
        // Register connection type
        services.AddScoped(providerType.ConnectionType);
        services.AddScoped(typeof(IDataConnection), serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(providerType.ConnectionType);
                
            return Activator.CreateInstance(
                providerType.ConnectionType,
                serviceProvider,
                logger) ?? throw new InvalidOperationException(
                    $"Failed to create instance of {providerType.ConnectionType.Name}");
        });
        
        // Register translator type
        services.AddScoped(providerType.TranslatorType);
        
        // Register configuration type if it's not the base type
        if (providerType.ConfigurationType != typeof(ConnectionConfiguration))
        {
            services.AddScoped(providerType.ConfigurationType);
        }
    }
}