using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FractalDataWorks.Services.ExternalConnections.Abstractions;

/// <summary>
/// Interface for external connections in the FractalDataWorks framework.
/// Provides common functionality for establishing and managing connections to external systems.
/// </summary>
/// <remarks>
/// External connections represent connections to databases, APIs, file systems, message queues,
/// and other external systems. Implementations should handle connection lifecycle, authentication,
/// and basic connectivity operations.
/// </remarks>
public interface IExternalConnection : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this connection.
    /// </summary>
    /// <value>A unique identifier for the connection instance.</value>
    /// <remarks>
    /// This identifier is used for connection tracking, pooling, and debugging purposes.
    /// It should remain constant for the lifetime of the connection instance.
    /// </remarks>
    string ConnectionId { get; }
    
    /// <summary>
    /// Gets the name of the connection provider (e.g., "SqlServer", "PostgreSQL", "REST API").
    /// </summary>
    /// <value>A string identifying the connection provider type.</value>
    string ProviderName { get; }
    
    /// <summary>
    /// Gets the current state of the connection.
    /// </summary>
    /// <value>The current connection state.</value>
    FdwConnectionState State { get; }
    
    /// <summary>
    /// Gets the connection string or configuration summary (sanitized for logging).
    /// </summary>
    /// <value>A sanitized version of the connection configuration suitable for logging.</value>
    /// <remarks>
    /// This property should return connection information with sensitive data (passwords, tokens)
    /// removed or masked for security purposes. Used for diagnostics and logging.
    /// </remarks>
    string ConnectionString { get; }
    
    /// <summary>
    /// Opens the connection asynchronously.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous open operation.
    /// The result indicates whether the connection was successfully opened.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the connection is already open or in an invalid state for opening.
    /// </exception>
    Task<IFdwResult> OpenAsync();
    
    /// <summary>
    /// Closes the connection asynchronously.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous close operation.
    /// The result indicates whether the connection was successfully closed.
    /// </returns>
    /// <remarks>
    /// This method should be idempotent - calling it multiple times should not cause errors.
    /// It should gracefully handle connections that are already closed.
    /// </remarks>
    Task<IFdwResult> CloseAsync();
    
    /// <summary>
    /// Tests the connection without fully opening it.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous test operation.
    /// The result indicates whether the connection test was successful.
    /// </returns>
    /// <remarks>
    /// This method performs a lightweight connectivity test to verify that the connection
    /// can be established without maintaining the connection state. Useful for validation
    /// and health checks.
    /// </remarks>
    Task<IFdwResult> TestConnectionAsync();
    
    /// <summary>
    /// Gets metadata about the connected system.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous metadata retrieval operation.
    /// The result contains system metadata if successful.
    /// </returns>
    /// <remarks>
    /// Metadata may include version information, capabilities, schema details, or other
    /// system-specific information useful for framework operations.
    /// </remarks>
    Task<IFdwResult<IConnectionMetadata>> GetMetadataAsync();
}

/// <summary>
/// Generic interface for external connections with typed configuration.
/// Extends the base connection interface with configuration-specific functionality.
/// </summary>
/// <typeparam name="TConfiguration">The type of configuration this connection requires.</typeparam>
/// <remarks>
/// Use this interface for connections that require specific configuration objects
/// beyond basic connection strings. The configuration provides type safety and
/// validation for connection parameters.
/// </remarks>
public interface IExternalConnection<TConfiguration> : IExternalConnection
    where TConfiguration : FdwConfigurationBase
{
    /// <summary>
    /// Gets the configuration object used by this connection.
    /// </summary>
    /// <value>The configuration object for this connection.</value>
    /// <remarks>
    /// This property provides access to the typed configuration used to create
    /// and configure the connection. The configuration is immutable after
    /// connection creation.
    /// </remarks>
    TConfiguration Configuration { get; }
    
    /// <summary>
    /// Initializes the connection with the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration object for this connection.</param>
    /// <returns>
    /// A task representing the asynchronous initialization operation.
    /// The result indicates whether initialization was successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection has already been initialized.</exception>
    /// <remarks>
    /// This method must be called before the connection can be used. It validates
    /// the configuration and prepares the connection for opening.
    /// </remarks>
    Task<IFdwResult> InitializeAsync(TConfiguration configuration);
}