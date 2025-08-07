using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace FractalDataWorks.Services.ExternalConnections.Abstractions;

/// <summary>
/// Interface for external connection factories in the FractalDataWorks framework.
/// Provides factory methods for creating and managing external connections.
/// </summary>
/// <remarks>
/// Connection factories abstract the creation of external connections, enabling
/// the framework to create connections without knowing the specific implementation details.
/// Factories handle connection configuration, validation, and lifecycle management.
/// </remarks>
public interface IExternalConnectionFactory
{
    /// <summary>
    /// Gets the name of the connection provider this factory creates.
    /// </summary>
    /// <value>The provider name (e.g., "SqlServer", "PostgreSQL", "MongoDB").</value>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the supported connection types this factory can create.
    /// </summary>
    /// <value>A collection of connection type names supported by this factory.</value>
    /// <remarks>
    /// Different providers may support multiple connection types (e.g., read-only, read-write,
    /// bulk operations). This property helps the framework select appropriate factories.
    /// </remarks>
    IReadOnlyList<string> SupportedConnectionTypes { get; }
    
    /// <summary>
    /// Gets the configuration type required by this factory.
    /// </summary>
    /// <value>The Type of configuration object required for connection creation.</value>
    Type ConfigurationType { get; }
    
    /// <summary>
    /// Creates a connection using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection.</param>
    /// <returns>
    /// A task representing the asynchronous connection creation operation.
    /// The result contains the created connection if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="configuration"/> is not of the expected type.
    /// </exception>
    /// <remarks>
    /// This method creates and initializes a new connection instance. The connection
    /// is not automatically opened - callers must call OpenAsync() separately.
    /// </remarks>
    Task<IFdwResult<IExternalConnection>> CreateConnectionAsync(FdwConfigurationBase configuration);
    
    /// <summary>
    /// Creates a connection using the provided configuration with connection type specification.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection.</param>
    /// <param name="connectionType">The specific type of connection to create.</param>
    /// <returns>
    /// A task representing the asynchronous connection creation operation.
    /// The result contains the created connection if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> or <paramref name="connectionType"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="configuration"/> is not of the expected type or
    /// <paramref name="connectionType"/> is not supported.
    /// </exception>
    /// <remarks>
    /// This overload allows specifying the exact type of connection to create when
    /// the factory supports multiple connection types.
    /// </remarks>
    Task<IFdwResult<IExternalConnection>> CreateConnectionAsync(FdwConfigurationBase configuration, string connectionType);
    
    /// <summary>
    /// Validates the provided configuration without creating a connection.
    /// </summary>
    /// <param name="configuration">The configuration object to validate.</param>
    /// <returns>
    /// A task representing the asynchronous validation operation.
    /// The result indicates whether the configuration is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <remarks>
    /// This method performs configuration validation without the overhead of creating
    /// an actual connection. Useful for configuration validation in setup processes.
    /// </remarks>
    Task<IFdwResult> ValidateConfigurationAsync(FdwConfigurationBase configuration);
    
    /// <summary>
    /// Tests connectivity using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection test.</param>
    /// <returns>
    /// A task representing the asynchronous connectivity test operation.
    /// The result indicates whether connectivity can be established.
    /// </returns>
    /// <remarks>
    /// This method creates a temporary connection, tests connectivity, and immediately
    /// disposes the connection. Useful for validation and health checks.
    /// </remarks>
    Task<IFdwResult> TestConnectivityAsync(FdwConfigurationBase configuration);
}

/// <summary>
/// Generic interface for external connection factories with typed configuration.
/// Extends the base factory interface with type-safe configuration handling.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this factory requires.</typeparam>
/// <typeparam name="TConnection">The type of connection this factory creates.</typeparam>
/// <remarks>
/// Use this interface for factories that work with specific configuration and connection types.
/// It provides type safety and eliminates the need for runtime type checking and casting.
/// </remarks>
public interface IExternalConnectionFactory<in TConfiguration, TConnection> : IExternalConnectionFactory
    where TConfiguration : FdwConfigurationBase
    where TConnection : class, IExternalConnection
{
    /// <summary>
    /// Creates a typed connection using the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection.</param>
    /// <returns>
    /// A task representing the asynchronous connection creation operation.
    /// The result contains the created connection if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <remarks>
    /// This method provides type-safe connection creation without the need for casting.
    /// The connection is not automatically opened - callers must call OpenAsync() separately.
    /// </remarks>
    Task<IFdwResult<TConnection>> CreateConnectionAsync(TConfiguration configuration);
    
    /// <summary>
    /// Creates a typed connection using the provided configuration with connection type specification.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection.</param>
    /// <param name="connectionType">The specific type of connection to create.</param>
    /// <returns>
    /// A task representing the asynchronous connection creation operation.
    /// The result contains the created connection if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configuration"/> or <paramref name="connectionType"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="connectionType"/> is not supported.
    /// </exception>
    /// <remarks>
    /// This overload allows specifying the exact type of connection to create when
    /// the factory supports multiple connection types, with full type safety.
    /// </remarks>
    Task<IFdwResult<TConnection>> CreateConnectionAsync(TConfiguration configuration, string connectionType);
    
    /// <summary>
    /// Validates the provided typed configuration without creating a connection.
    /// </summary>
    /// <param name="configuration">The configuration object to validate.</param>
    /// <returns>
    /// A task representing the asynchronous validation operation.
    /// The result indicates whether the configuration is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <remarks>
    /// This method performs type-safe configuration validation without the overhead
    /// of creating an actual connection.
    /// </remarks>
    Task<IFdwResult> ValidateConfigurationAsync(TConfiguration configuration);
    
    /// <summary>
    /// Tests connectivity using the provided typed configuration.
    /// </summary>
    /// <param name="configuration">The configuration object for the connection test.</param>
    /// <returns>
    /// A task representing the asynchronous connectivity test operation.
    /// The result indicates whether connectivity can be established.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <remarks>
    /// This method creates a temporary connection, tests connectivity, and immediately
    /// disposes the connection using the typed configuration.
    /// </remarks>
    Task<IFdwResult> TestConnectivityAsync(TConfiguration configuration);
}