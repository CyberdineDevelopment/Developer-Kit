using System;
using FractalDataWorks.Services.DataProvider.MsSql.EnhancedEnums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FractalDataWorks.Services.DataProvider.MsSql.Extensions;

/// <summary>
/// Extension methods for registering Microsoft SQL Server data provider services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default Microsoft SQL Server data provider service to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    public static IServiceCollection AddMsSqlDataProvider(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        MsSqlDataProviderServiceType.Default.Add(services, configuration);
        return services;
    }

    /// <summary>
    /// Adds the specified Microsoft SQL Server data provider service type to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services, serviceType, or configuration is null.</exception>
    public static IServiceCollection AddMsSqlDataProvider(
        this IServiceCollection services,
        MsSqlDataProviderServiceType serviceType,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (serviceType == null)
            throw new ArgumentNullException(nameof(serviceType));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        serviceType.Add(services, configuration);
        return services;
    }

    /// <summary>
    /// Adds multiple Microsoft SQL Server data provider service types to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <param name="serviceTypes">The service types to register.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services, configuration, or serviceTypes is null.</exception>
    public static IServiceCollection AddMsSqlDataProviders(
        this IServiceCollection services,
        IConfiguration configuration,
        params MsSqlDataProviderServiceType[] serviceTypes)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        if (serviceTypes == null)
            throw new ArgumentNullException(nameof(serviceTypes));

        foreach (var serviceType in serviceTypes)
        {
            if (serviceType != null)
            {
                serviceType.Add(services, configuration);
            }
        }

        return services;
    }

    /// <summary>
    /// Adds all available Microsoft SQL Server data provider service types to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    public static IServiceCollection AddAllMsSqlDataProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return services.AddMsSqlDataProviders(configuration,
            MsSqlDataProviderServiceType.Default,
            MsSqlDataProviderServiceType.ReadOnly,
            MsSqlDataProviderServiceType.HighPerformance,
            MsSqlDataProviderServiceType.Reporting,
            MsSqlDataProviderServiceType.Transactional);
    }

    /// <summary>
    /// Adds the default Microsoft SQL Server data provider service to the host application builder.
    /// </summary>
    /// <param name="builder">The host application builder to add services to.</param>
    /// <returns>The host application builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static IHostApplicationBuilder AddMsSqlDataProvider(this IHostApplicationBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        MsSqlDataProviderServiceType.Default.Add(builder);
        return builder;
    }

    /// <summary>
    /// Adds the specified Microsoft SQL Server data provider service type to the host application builder.
    /// </summary>
    /// <param name="builder">The host application builder to add services to.</param>
    /// <param name="serviceType">The service type to register.</param>
    /// <returns>The host application builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or serviceType is null.</exception>
    public static IHostApplicationBuilder AddMsSqlDataProvider(
        this IHostApplicationBuilder builder,
        MsSqlDataProviderServiceType serviceType)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (serviceType == null)
            throw new ArgumentNullException(nameof(serviceType));

        serviceType.Add(builder);
        return builder;
    }

    /// <summary>
    /// Adds multiple Microsoft SQL Server data provider service types to the host application builder.
    /// </summary>
    /// <param name="builder">The host application builder to add services to.</param>
    /// <param name="serviceTypes">The service types to register.</param>
    /// <returns>The host application builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or serviceTypes is null.</exception>
    public static IHostApplicationBuilder AddMsSqlDataProviders(
        this IHostApplicationBuilder builder,
        params MsSqlDataProviderServiceType[] serviceTypes)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (serviceTypes == null)
            throw new ArgumentNullException(nameof(serviceTypes));

        foreach (var serviceType in serviceTypes)
        {
            if (serviceType != null)
            {
                serviceType.Add(builder);
            }
        }

        return builder;
    }

    /// <summary>
    /// Adds all available Microsoft SQL Server data provider service types to the host application builder.
    /// </summary>
    /// <param name="builder">The host application builder to add services to.</param>
    /// <returns>The host application builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static IHostApplicationBuilder AddAllMsSqlDataProviders(this IHostApplicationBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        return builder.AddMsSqlDataProviders(
            MsSqlDataProviderServiceType.Default,
            MsSqlDataProviderServiceType.ReadOnly,
            MsSqlDataProviderServiceType.HighPerformance,
            MsSqlDataProviderServiceType.Reporting,
            MsSqlDataProviderServiceType.Transactional);
    }

    /// <summary>
    /// Adds the default Microsoft SQL Server data provider service to the host builder.
    /// </summary>
    /// <param name="builder">The host builder to add services to.</param>
    /// <returns>The host builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static IHostBuilder AddMsSqlDataProvider(this IHostBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        MsSqlDataProviderServiceType.Default.Add(builder);
        return builder;
    }

    /// <summary>
    /// Adds the specified Microsoft SQL Server data provider service type to the host builder.
    /// </summary>
    /// <param name="builder">The host builder to add services to.</param>
    /// <param name="serviceType">The service type to register.</param>
    /// <returns>The host builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or serviceType is null.</exception>
    public static IHostBuilder AddMsSqlDataProvider(
        this IHostBuilder builder,
        MsSqlDataProviderServiceType serviceType)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (serviceType == null)
            throw new ArgumentNullException(nameof(serviceType));

        serviceType.Add(builder);
        return builder;
    }

    /// <summary>
    /// Adds multiple Microsoft SQL Server data provider service types to the host builder.
    /// </summary>
    /// <param name="builder">The host builder to add services to.</param>
    /// <param name="serviceTypes">The service types to register.</param>
    /// <returns>The host builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or serviceTypes is null.</exception>
    public static IHostBuilder AddMsSqlDataProviders(
        this IHostBuilder builder,
        params MsSqlDataProviderServiceType[] serviceTypes)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (serviceTypes == null)
            throw new ArgumentNullException(nameof(serviceTypes));

        foreach (var serviceType in serviceTypes)
        {
            if (serviceType != null)
            {
                serviceType.Add(builder);
            }
        }

        return builder;
    }

    /// <summary>
    /// Adds all available Microsoft SQL Server data provider service types to the host builder.
    /// </summary>
    /// <param name="builder">The host builder to add services to.</param>
    /// <returns>The host builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static IHostBuilder AddAllMsSqlDataProviders(this IHostBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        return builder.AddMsSqlDataProviders(
            MsSqlDataProviderServiceType.Default,
            MsSqlDataProviderServiceType.ReadOnly,
            MsSqlDataProviderServiceType.HighPerformance,
            MsSqlDataProviderServiceType.Reporting,
            MsSqlDataProviderServiceType.Transactional);
    }
}