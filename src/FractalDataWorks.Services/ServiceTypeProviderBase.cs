using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FractalDataWorks.Configuration.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Base class for service providers that manage collections of service types and create service instances.
/// </summary>
/// <typeparam name="TService">The service interface type.</typeparam>
/// <typeparam name="TServiceType">The service type that inherits from ServiceTypeBase.</typeparam>
/// <typeparam name="TConfiguration">The configuration type.</typeparam>
/// <ExcludeFromTest>Abstract base class for service providers with no business logic to test</ExcludeFromTest>
[ExcludeFromCodeCoverage(Justification = "Abstract base class for service providers with no business logic")]
public abstract class ServiceTypeProviderBase<TService, TServiceType, TConfiguration>
    where TService : class, IFdwService
    where TServiceType : ServiceTypeBase<TService, TConfiguration>
    where TConfiguration : class, IFdwConfiguration
{
    private readonly IConfigurationRegistry<TConfiguration> _configurationRegistry;
    private readonly ILogger<ServiceTypeProviderBase<TService, TServiceType, TConfiguration>> _logger;
    private readonly Dictionary<string, TServiceType> _serviceTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeProviderBase{TService,TServiceType,TConfiguration}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurationRegistry">The configuration registry.</param>
    /// <param name="serviceTypes">The collection of available service types.</param>
    protected ServiceTypeProviderBase(
        ILogger<ServiceTypeProviderBase<TService, TServiceType, TConfiguration>> logger,
        IConfigurationRegistry<TConfiguration> configurationRegistry,
        IEnumerable<TServiceType> serviceTypes)
    {
        _logger = logger;
        _configurationRegistry = configurationRegistry;
        _serviceTypes = serviceTypes.ToDictionary(st => st.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceTypeProviderBase{TService,TServiceType,TConfiguration}"/> class with null logger.
    /// </summary>
    /// <param name="configurationRegistry">The configuration registry.</param>
    /// <param name="serviceTypes">The collection of available service types.</param>
    protected ServiceTypeProviderBase(
        IConfigurationRegistry<TConfiguration> configurationRegistry,
        IEnumerable<TServiceType> serviceTypes)
    {
        _logger = NullLogger<ServiceTypeProviderBase<TService, TServiceType, TConfiguration>>.Instance;
        _configurationRegistry = configurationRegistry;
        _serviceTypes = serviceTypes.ToDictionary(st => st.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the available service types.
    /// </summary>
    public IReadOnlyDictionary<string, TServiceType> ServiceTypes => _serviceTypes;

    /// <summary>
    /// Gets a service instance for the specified service type name.
    /// </summary>
    /// <param name="serviceTypeName">The name of the service type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the service instance or an error.</returns>
    public virtual async Task<IFdwResult<TService>> GetServiceAsync(
        string serviceTypeName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(serviceTypeName))
        {
            ServiceProviderBaseLog.InvalidServiceTypeName(_logger);
            return FdwResult<TService>.Failure("Service type name cannot be null or empty");
        }

        if (!_serviceTypes.TryGetValue(serviceTypeName, out var serviceType))
        {
            ServiceProviderBaseLog.ServiceTypeNotFound(_logger, serviceTypeName);
            return FdwResult<TService>.Failure($"Service type '{serviceTypeName}' not found");
        }

        // Get configuration for this service type
        var configResult = await GetConfigurationAsync(serviceTypeName, cancellationToken).ConfigureAwait(false);
        if (configResult.IsFailure)
        {
            ServiceProviderBaseLog.ConfigurationRetrievalFailed(_logger, serviceTypeName, configResult.Message ?? "Unknown error");
            return FdwResult<TService>.Failure(configResult.Message ?? "unknown error");
        }

        // Use the service type's factory method to create the service
        var serviceResult = serviceType.Create(configResult.Value!);
        if (serviceResult.IsFailure)
        {
            ServiceProviderBaseLog.ServiceCreationFailed(_logger, serviceTypeName, serviceResult.Message ?? "Unknown error");
            return FdwResult<TService>.Failure(serviceResult.Message ?? "unknown error");
        }

        ServiceProviderBaseLog.ServiceCreated(_logger, serviceTypeName);
        return FdwResult<TService>.Success(serviceResult.Value!);
    }

    /// <summary>
    /// Gets a service instance for the specified service type.
    /// </summary>
    /// <param name="serviceType">The service type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the service instance or an error.</returns>
    public virtual Task<IFdwResult<TService>> GetServiceAsync(
        TServiceType serviceType,
        CancellationToken cancellationToken = default)
    {
        if (serviceType == null)
        {
            ServiceProviderBaseLog.NullServiceType(_logger);
            return Task.FromResult(FdwResult<TService>.Failure("Service type cannot be null"));
        }

        return GetServiceAsync(serviceType.Name, cancellationToken);
    }

    /// <summary>
    /// Gets the configuration for the specified service type.
    /// </summary>
    /// <param name="serviceTypeName">The name of the service type.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the configuration or an error.</returns>
    protected virtual Task<IFdwResult<TConfiguration>> GetConfigurationAsync(
        string serviceTypeName,
        CancellationToken cancellationToken)
    {
        // Default implementation tries to get the first available configuration
        // Override this method to provide custom configuration logic
        var configs = _configurationRegistry.GetAll();
        var config = configs.FirstOrDefault();

        if (config == null)
        {
            return Task.FromResult(FdwResult<TConfiguration>.Failure($"Configuration not found for service type '{serviceTypeName}'"));
        }

        return Task.FromResult(FdwResult<TConfiguration>.Success(config));
    }

    /// <summary>
    /// Validates that the provider has been properly initialized.
    /// </summary>
    /// <returns>A result indicating success or failure.</returns>
    public virtual IFdwResult ValidateProvider()
    {
        if (_serviceTypes == null || _serviceTypes.Count == 0)
        {
            return FdwResult.Failure("No service types registered");
        }

        if (_configurationRegistry == null)
        {
            return FdwResult.Failure("Configuration registry not initialized");
        }

        return FdwResult.Success();
    }

    /// <summary>
    /// Gets all available service instances.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of service results.</returns>
    public virtual async Task<IReadOnlyList<IFdwResult<TService>>> GetAllServicesAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<IFdwResult<TService>>();

        foreach (var serviceType in _serviceTypes.Values)
        {
            var result = await GetServiceAsync(serviceType, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }

        return results;
    }
}