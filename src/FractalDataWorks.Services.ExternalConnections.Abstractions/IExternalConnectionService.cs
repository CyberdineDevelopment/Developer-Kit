using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions;

/// <summary>
/// Interface for external connection services in the FractalDataWorks framework.
/// Provides enhanced functionality for connection management and service integration.
/// </summary>
/// <remarks>
/// Connection services extend the basic service type pattern with connection-specific
/// functionality. They serve as the bridge between the framework's service discovery
/// system and actual connection implementations.
/// </remarks>
public interface IExternalConnectionService : IServiceType
{
    /// <summary>
    /// Gets the data store names that this connection service supports.
    /// </summary>
    /// <value>An array of data store names (e.g., "SqlServer", "PostgreSQL", "MongoDB").</value>
    /// <remarks>
    /// This property enables the framework to match connection services with
    /// specific data store requirements. Providers may support multiple data stores
    /// if they use common protocols or drivers.
    /// </remarks>
    string[] SupportedDataStores { get; }
    
    /// <summary>
    /// Gets the service name for this connection type (e.g., "Microsoft.Data.SqlClient", "Npgsql").
    /// </summary>
    /// <value>The underlying service or driver name.</value>
    /// <remarks>
    /// This typically corresponds to the NuGet package or driver name used for
    /// the actual connection implementation. Used for diagnostics and compatibility checks.
    /// </remarks>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the concrete connection type (e.g., SqlConnection, NpgsqlConnection).
    /// </summary>
    /// <value>The Type of the underlying connection implementation.</value>
    /// <remarks>
    /// This property provides access to the actual connection type for advanced
    /// scenarios where framework code needs to work with specific connection implementations.
    /// </remarks>
    Type ConnectionType { get; }
    
    /// <summary>
    /// Gets the configuration type required for this connection service.
    /// </summary>
    /// <value>The Type of configuration object required for connection creation.</value>
    /// <remarks>
    /// This enables the framework to validate configuration objects and provide
    /// type-safe configuration handling for different connection services.
    /// </remarks>
    Type ConfigurationType { get; }
    
    /// <summary>
    /// Gets the supported connection modes for this service.
    /// </summary>
    /// <value>A collection of connection mode names supported by this service.</value>
    /// <remarks>
    /// Connection modes may include "ReadOnly", "ReadWrite", "Bulk", "Streaming", etc.
    /// This information helps the framework select appropriate services for specific use cases.
    /// </remarks>
    IReadOnlyList<string> SupportedConnectionModes { get; }
    
    /// <summary>
    /// Gets the priority of this service when multiple services support the same data store.
    /// </summary>
    /// <value>A numeric priority value where higher numbers indicate higher priority.</value>
    /// <remarks>
    /// When multiple services can handle the same data store, the framework uses this
    /// priority to select the preferred service. This allows for fallback scenarios
    /// and service preferences.
    /// </remarks>
    int Priority { get; }
    
    /// <summary>
    /// Creates a connection factory instance using the provided service service.
    /// </summary>
    /// <param name="serviceProvider">The service service for dependency resolution.</param>
    /// <returns>
    /// A task representing the asynchronous factory creation operation.
    /// The result contains the connection factory if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    /// <remarks>
    /// This method creates a factory instance that can create connections of this service's type.
    /// The factory handles the actual connection creation and management logic.
    /// </remarks>
    Task<IFdwResult<IExternalConnectionFactory>> CreateConnectionFactoryAsync(IServiceProvider serviceProvider);
    
    /// <summary>
    /// Validates whether this service can handle the specified data store and connection mode.
    /// </summary>
    /// <param name="dataStore">The data store name to validate.</param>
    /// <param name="connectionMode">The connection mode to validate (optional).</param>
    /// <returns>
    /// A result indicating whether this service can handle the specified requirements.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dataStore"/> is null.</exception>
    /// <remarks>
    /// This method enables dynamic service selection based on runtime requirements.
    /// It allows the framework to validate service compatibility before attempting
    /// to create connections.
    /// </remarks>
    IFdwResult ValidateCapability(string dataStore, string? connectionMode = null);
    
    /// <summary>
    /// Gets metadata about this service's capabilities and features.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous metadata retrieval operation.
    /// The result contains service metadata if successful.
    /// </returns>
    /// <remarks>
    /// Provider metadata includes information about supported features, version information,
    /// performance characteristics, and other details useful for framework operations.
    /// </remarks>
    Task<IFdwResult<IProviderMetadata>> GetProviderMetadataAsync();
}

/// <summary>
/// Generic interface for external connection services with typed configuration.
/// Extends the base service interface with type-safe configuration handling.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this service requires.</typeparam>
/// <remarks>
/// Use this interface for services that work with specific configuration types.
/// It provides compile-time type safety and eliminates runtime type checking.
/// </remarks>
public interface IExternalConnectionService<TConfiguration> : IExternalConnectionService
    where TConfiguration : FdwConfigurationBase
{
    /// <summary>
    /// Creates a typed connection factory instance using the provided service service.
    /// </summary>
    /// <param name="serviceProvider">The service service for dependency resolution.</param>
    /// <returns>
    /// A task representing the asynchronous factory creation operation.
    /// The result contains the typed connection factory if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    /// <remarks>
    /// This method creates a type-safe factory instance that works specifically with
    /// the service's configuration type, eliminating the need for runtime casting.
    /// </remarks>
    Task<IFdwResult<IExternalConnectionFactory<TConfiguration, IExternalConnection<TConfiguration>>>> CreateTypedConnectionFactoryAsync(
        IServiceProvider serviceProvider);
    
    /// <summary>
    /// Validates the provided configuration for this service.
    /// </summary>
    /// <param name="configuration">The configuration object to validate.</param>
    /// <returns>
    /// A result indicating whether the configuration is valid for this service.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <remarks>
    /// This method performs service-specific configuration validation beyond the
    /// basic validation provided by the configuration object itself.
    /// </remarks>
    IFdwResult ValidateConfiguration(TConfiguration configuration);
}