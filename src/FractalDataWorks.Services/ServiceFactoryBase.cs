using System;
using System.Globalization;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Base implementation of the service factory with comprehensive type-safe creation patterns.
/// Provides a complete foundation for service factories with automatic configuration validation,
/// type checking, and structured logging support.
/// </summary>
/// <typeparam name="TService">The type of service this factory creates.</typeparam>
/// <typeparam name="TConfiguration">The configuration type required by the service.</typeparam>
public abstract class ServiceFactoryBase<TService, TConfiguration> : 
    IServiceFactory,
    IServiceFactory<TService>,
    IServiceFactory<TService, TConfiguration>
    where TService : class
    where TConfiguration : class, IFdwConfiguration
{
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the logger instance for derived classes.
    /// </summary>
    protected ILogger Logger => _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceFactoryBase{TService, TConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance. If null, uses Microsoft's NullLogger.</param>
    protected ServiceFactoryBase(ILogger? logger)
    {
        // Use Microsoft's NullLogger for consistency with ILogger abstractions
        // This works seamlessly when Serilog is registered via services.AddSerilog()
        _logger = logger ?? NullLogger.Instance;
    }

    #region Abstract Core Methods

    /// <summary>
    /// Creates a service instance with the specified strongly-typed configuration.
    /// Default implementation creates a new instance using the constructor.
    /// Override this method for custom creation logic.
    /// </summary>
    /// <param name="configuration">The strongly-typed configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    protected virtual IFdwResult<TService> CreateCore(TConfiguration configuration)
    {
        try
        {
            var service = (TService)Activator.CreateInstance(typeof(TService), configuration)!;
            return FdwResult<TService>.Success(service);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service of type {ServiceType}", typeof(TService).Name);
            return FdwResult<TService>.Failure($"Failed to create service: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a service instance for the specified configuration name.
    /// </summary>
    /// <param name="configurationName">The name of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the service instance.</returns>
    public abstract Task<TService> GetService(string configurationName);

    /// <summary>
    /// Creates a service instance for the specified configuration ID.
    /// </summary>
    /// <param name="configurationId">The ID of the configuration to use.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the service instance.</returns>
    public abstract Task<TService> GetService(int configurationId);

    #endregion

    #region Configuration Validation

    /// <summary>
    /// Validates and casts a configuration to the expected type.
    /// </summary>
    /// <param name="configuration">The configuration to validate.</param>
    /// <param name="validConfiguration">The valid configuration if successful.</param>
    /// <returns>The validation result.</returns>
    protected IFdwResult<TConfiguration> ValidateConfiguration(
        IFdwConfiguration? configuration,
        out TConfiguration? validConfiguration)
    {
        if (configuration == null)
        {
            ServiceBaseLog.InvalidConfigurationWarning(_logger, 
                "Configuration cannot be null");
            validConfiguration = null;
            return FdwResult<TConfiguration>.Failure("Configuration cannot be null");
        }

        if (configuration is TConfiguration config && config.Validate())
        {
            validConfiguration = config;
            return FdwResult<TConfiguration>.Success(config);
        }

        ServiceBaseLog.InvalidConfigurationWarning(_logger, 
            string.Format(CultureInfo.InvariantCulture,
                "Invalid configuration type. Expected {0}, got {1}",
                typeof(TConfiguration).Name,
                configuration.GetType().Name));

        validConfiguration = null;
        return FdwResult<TConfiguration>.Failure(
            "Invalid configuration type");
    }

    #endregion

    #region IServiceFactory Implementation (Non-Generic)

    /// <summary>
    /// Creates a service instance of the specified type.
    /// This method checks if the requested type matches the factory's service type.
    /// </summary>
    /// <typeparam name="T">The type of service to create.</typeparam>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public IFdwResult<T> Create<T>(IFdwConfiguration configuration) where T : IFdwService
    {
        // Check if the requested type is assignable from our service type
        if (!typeof(T).IsAssignableFrom(typeof(TService)))
        {
            ServiceBaseLog.InvalidConfigurationWarning(_logger,
                string.Format(CultureInfo.InvariantCulture,
                    "Invalid service type. Expected {0} or compatible type, got {1}",
                    typeof(TService).Name,
                    typeof(T).Name));

            return FdwResult<T>.Failure(
                "Invalid service type");
        }

        // Validate configuration and create service
        var validationResult = ValidateConfiguration(configuration, out var validConfig);
        if (validationResult.Error || validConfig == null)
        {
            return FdwResult<T>.Failure(validationResult.Message ?? "Configuration validation failed");
        }

        var serviceResult = CreateCore(validConfig);
        if (serviceResult.Error || serviceResult.Value == null)
        {
            return FdwResult<T>.Failure(serviceResult.Message ?? "Service creation failed");
        }

        // Cast the service to the requested type
        if (serviceResult.Value is T typedService)
        {
            return FdwResult<T>.Success(typedService);
        }

        return FdwResult<T>.Failure("Service type cast failed");
    }

    /// <summary>
    /// Creates a service instance and returns it as IFdwService.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    IFdwResult<IFdwService> IServiceFactory.Create(IFdwConfiguration configuration)
    {
        // Validate configuration and create service
        var validationResult = ValidateConfiguration(configuration, out var validConfig);
        if (validationResult.Error || validConfig == null)
        {
            return FdwResult<IFdwService>.Failure(validationResult.Message ?? "Configuration validation failed");
        }

        var serviceResult = CreateCore(validConfig);
        if (serviceResult.Error || serviceResult.Value == null)
        {
            return FdwResult<IFdwService>.Failure(serviceResult.Message ?? "Service creation failed");
        }

        // Cast service to IFdwService
        if (serviceResult.Value is IFdwService fdwService)
        {
            return FdwResult<IFdwService>.Success(fdwService);
        }

        return FdwResult<IFdwService>.Failure(
            string.Format(CultureInfo.InvariantCulture,
                "Service type {0} does not implement IFdwService",
                typeof(TService).Name));
    }

    #endregion

    #region IServiceFactory<TService> Implementation

    /// <summary>
    /// Creates a service instance with configuration validation.
    /// This method validates that the configuration is of the correct type before creation.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    IFdwResult<TService> IServiceFactory<TService>.Create(IFdwConfiguration configuration)
    {
        // Validate configuration and create service
        var validationResult = ValidateConfiguration(configuration, out var validConfig);
        if (validationResult.Error || validConfig == null)
        {
            return FdwResult<TService>.Failure(validationResult.Message ?? "Configuration validation failed");
        }

        return CreateCore(validConfig);
    }

    #endregion

    #region IServiceFactory<TService, TConfiguration> Implementation

    /// <summary>
    /// Creates a service instance with the specified strongly-typed configuration.
    /// </summary>
    /// <param name="configuration">The configuration for the service.</param>
    /// <returns>A result containing the created service or an error message.</returns>
    public IFdwResult<TService> Create(TConfiguration configuration)
    {
        if (configuration == null)
        {
            ServiceBaseLog.InvalidConfigurationWarning(_logger, "Configuration cannot be null");
            return FdwResult<TService>.Failure("Configuration cannot be null");
        }

        if (!configuration.Validate())
        {
            ServiceBaseLog.InvalidConfigurationWarning(_logger, 
                "Configuration validation failed");
            return FdwResult<TService>.Failure("Configuration validation failed");
        }

        return CreateCore(configuration);
    }

    #endregion
}
