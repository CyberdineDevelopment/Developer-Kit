using System;
using System.Collections.Generic;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.EnhancedEnums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace FractalDataWorks.Services.EnhancedEnums;

/// <summary>
/// Base class for service type options that inherit from EnumOptionBase and provide self-registration capabilities.
/// </summary>
/// <typeparam name="TDerived">The derived service type option class.</typeparam>
/// <typeparam name="TService">The service interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the service.</typeparam>
/// <typeparam name="TFactory">The factory type for creating service instances.</typeparam>
public abstract class ServiceTypeOptionBase<TDerived, TService, TConfiguration, TFactory> : EnumOptionBase<TDerived>
    where TDerived : ServiceTypeOptionBase<TDerived, TService, TConfiguration, TFactory>
    where TService : class, IFdwService
    where TConfiguration : class, IFdwConfiguration, new()
    where TFactory : class, IServiceFactory<TService, TConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeOptionBase{TDerived, TService, TConfiguration, TFactory}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type option.</param>
    /// <param name="name">The name of this service type option.</param>
    protected ServiceTypeOptionBase(int id, string name) : base(id, name)
    {
    }

    /// <summary>
    /// Gets the configuration section name for this service type.
    /// Override to customize the configuration section path.
    /// </summary>
    protected virtual string ConfigurationSection => $"{typeof(TService).Name}:Configurations";

    /// <summary>
    /// Registers the service type with the dependency injection container.
    /// Override to completely customize the registration logic.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    protected virtual void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration as a list bound to the section
        services.Configure<List<TConfiguration>>(options =>
        {
            var section = configuration.GetSection(ConfigurationSection);
            section.Bind(options);
        });

        // Register IConfigurationRegistry
        services.AddSingleton<IConfigurationRegistry<TConfiguration>>(provider =>
        {
            var monitor = provider.GetRequiredService<IOptionsMonitor<List<TConfiguration>>>();
            return new ConfigurationRegistryCore<TConfiguration>(monitor.CurrentValue);
        });

        // Register the factory as singleton at all interface levels
        services.AddSingleton<TFactory>();
        services.AddSingleton<IServiceFactory<TService, TConfiguration>>(provider =>
            provider.GetRequiredService<TFactory>());
        services.AddSingleton<IServiceFactory<TService>>(provider =>
            provider.GetRequiredService<TFactory>());
        services.AddSingleton<IServiceFactory>(provider =>
            provider.GetRequiredService<TFactory>());

        // Register service resolver that uses the factory
        services.AddTransient<TService>(provider =>
        {
            var factory = provider.GetRequiredService<TFactory>();
            var registry = provider.GetRequiredService<IConfigurationRegistry<TConfiguration>>();

            // Get configuration for this specific service type by ID
            var config = registry.Get(Id);

            if (config == null)
                throw new InvalidOperationException($"No configuration found for service type '{Name}' with ID {Id}");

            // Check if configuration is enabled (if it has an IsEnabled property)
            var configType = config.GetType();
            var isEnabledProperty = configType.GetProperty("IsEnabled");
            if (isEnabledProperty != null && isEnabledProperty.GetValue(config) is bool isEnabled && !isEnabled)
                throw new InvalidOperationException($"No enabled configuration found for service type '{Name}' with ID {Id}");

            var result = factory.Create(config);
            if (!result.IsSuccess)
                throw new InvalidOperationException($"Failed to create service '{Name}': {result.Message}");

            return result.Value!;
        });
    }

    /// <summary>
    /// Override to add additional service-specific registrations.
    /// Called after the core registration logic.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    protected virtual void RegisterAdditional(IServiceCollection services, IConfiguration configuration)
    {
        // Override in derived classes for additional registrations
    }

    // ========== PUBLIC ADD METHODS ==========

    /// <summary>
    /// Adds this service type to an IHostApplicationBuilder.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    public void Add(IHostApplicationBuilder builder)
    {
        Register(builder.Services, builder.Configuration);
        RegisterAdditional(builder.Services, builder.Configuration);
    }


    /// <summary>
    /// Adds this service type to an IHostBuilder.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    public void Add(IHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            Register(services, context.Configuration);
            RegisterAdditional(services, context.Configuration);
        });
    }


    /// <summary>
    /// Adds this service type to an IServiceCollection with configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    public void Add(IServiceCollection services, IConfiguration configuration)
    {
        Register(services, configuration);
        RegisterAdditional(services, configuration);
    }


}