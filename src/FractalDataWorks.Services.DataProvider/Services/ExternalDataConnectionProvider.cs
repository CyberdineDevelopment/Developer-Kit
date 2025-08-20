using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.DataProvider.Services;

/// <summary>
/// Concrete implementation of IExternalDataConnectionProvider that manages named data connections.
/// </summary>
/// <remarks>
/// This provider acts as a registry and router for external data connections, allowing
/// the system to manage multiple named connections and route commands to the appropriate
/// connection based on the command's ConnectionName property.
/// 
/// Key responsibilities:
/// - Maintain a registry of named connections
/// - Route data commands to appropriate connections
/// - Provide schema discovery across all connections
/// - Monitor connection health and availability
/// - Aggregate connection metadata
/// </remarks>
public sealed class ExternalDataConnectionProvider : IExternalDataConnectionProvider
{
    private readonly ConcurrentDictionary<string, object> _connections;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExternalDataConnectionProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalDataConnectionProvider"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency injection.</param>
    /// <param name="logger">Logger for this provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
    public ExternalDataConnectionProvider(
        IServiceProvider serviceProvider,
        ILogger<ExternalDataConnectionProvider> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connections = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);

        _logger.LogInformation("ExternalDataConnectionProvider initialized");
    }

    /// <summary>
    /// Gets a specific named connection.
    /// </summary>
    /// <param name="connectionName">The name of the connection to retrieve.</param>
    /// <returns>The connection instance if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when connectionName is null or empty.</exception>
    private object? GetConnection(string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException("Connection name cannot be null or empty.", nameof(connectionName));

        return _connections.TryGetValue(connectionName, out var connection) ? connection : null;
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<T>> ExecuteCommand<T>(DataCommandBase command, CancellationToken cancellationToken = default)
    {
        if (command == null)
        {
            const string errorMessage = "Command cannot be null";
            _logger.LogError("ExecuteCommand called with null command");
            return FdwResult<T>.Failure(errorMessage);
        }

        if (string.IsNullOrWhiteSpace(command.ConnectionName))
        {
            const string errorMessage = "Command must specify a valid connection name";
            _logger.LogError("ExecuteCommand called with null or empty connection name");
            return FdwResult<T>.Failure(errorMessage);
        }

        // Validate the command first
        var validationResult = command.Validate();
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Format(
                CultureInfo.InvariantCulture,
                "Command validation failed: {0}",
                string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            
            _logger.LogWarning(
                "Command validation failed for {CommandType} on connection {ConnectionName}: {ErrorMessage}",
                command.GetType().Name,
                command.ConnectionName,
                string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
            
            return FdwResult<T>.Failure(errorMessage);
        }

        using (_logger.BeginScope(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Operation"] = "ExecuteCommand",
            ["CommandType"] = command.GetType().Name,
            ["ConnectionName"] = command.ConnectionName,
            ["CommandId"] = command.CommandId,
            ["CorrelationId"] = command.CorrelationId
        }))
        {
            _logger.LogDebug(
                "Executing {CommandType} command on connection {ConnectionName}",
                command.GetType().Name,
                command.ConnectionName);

            try
            {
                var connectionObj = GetConnection(command.ConnectionName);
                if (connectionObj == null)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' not found",
                        command.ConnectionName);

                    _logger.LogError(
                        "Connection {ConnectionName} not found for command {CommandType}",
                        command.ConnectionName,
                        command.GetType().Name);

                    return FdwResult<T>.Failure(errorMessage);
                }

                // Try to cast to the expected interface
                if (connectionObj is not IExternalDataConnection<IExternalConnectionConfiguration> connection)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' does not implement the expected interface",
                        command.ConnectionName);

                    _logger.LogError(
                        "Connection {ConnectionName} does not implement IExternalDataConnection interface",
                        command.ConnectionName);

                    return FdwResult<T>.Failure(errorMessage);
                }

                // Test connection availability before executing
                var connectionTest = await connection.TestConnection(cancellationToken).ConfigureAwait(false);
                if (!connectionTest.IsSuccess || !connectionTest.Value)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' is not available: {1}",
                        command.ConnectionName,
                        connectionTest.Message ?? "Unknown reason");

                    _logger.LogError(
                        "Connection {ConnectionName} is not available for command {CommandType}: {ErrorMessage}",
                        command.ConnectionName,
                        command.GetType().Name,
                        connectionTest.Message);

                    return FdwResult<T>.Failure(errorMessage);
                }

                var result = await connection.Execute<T>(command, cancellationToken).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    _logger.LogDebug(
                        "Successfully executed {CommandType} command on connection {ConnectionName}",
                        command.GetType().Name,
                        command.ConnectionName);
                }
                else
                {
                    _logger.LogWarning(
                        "Command execution failed for {CommandType} on connection {ConnectionName}: {ErrorMessage}",
                        command.GetType().Name,
                        command.ConnectionName,
                        result.Message);
                }

                return result;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Command execution failed for {0} on connection {1}: {2}",
                    command.GetType().Name,
                    command.ConnectionName,
                    ex.Message);

                _logger.LogError(ex,
                    "Exception occurred during command execution for {CommandType} on connection {ConnectionName}",
                    command.GetType().Name,
                    command.ConnectionName);

                return FdwResult<T>.Failure(errorMessage);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IEnumerable<DataContainer>>> DiscoverConnectionSchema(
        string connectionName, 
        DataPath? startPath = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
        {
            const string errorMessage = "Connection name cannot be null or empty";
            _logger.LogError("DiscoverConnectionSchema called with null or empty connection name");
            return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
        }

        using (_logger.BeginScope(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Operation"] = "DiscoverSchema",
            ["ConnectionName"] = connectionName,
            ["StartPath"] = startPath?.ToString() ?? "null"
        }))
        {
            _logger.LogInformation(
                "Discovering schema for connection {ConnectionName} starting from path {StartPath}",
                connectionName,
                startPath?.ToString() ?? "root");

            try
            {
                var connectionObj = GetConnection(connectionName);
                if (connectionObj == null)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' not found",
                        connectionName);

                    _logger.LogError(
                        "Connection {ConnectionName} not found for schema discovery",
                        connectionName);

                    return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
                }

                // Try to cast to the expected interface
                if (connectionObj is not IExternalDataConnection<IExternalConnectionConfiguration> connection)
                {
                    var errorMessage = string.Format(
                        CultureInfo.InvariantCulture,
                        "Connection '{0}' does not implement the expected interface",
                        connectionName);

                    _logger.LogError(
                        "Connection {ConnectionName} does not implement IExternalDataConnection interface",
                        connectionName);

                    return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
                }

                var result = await connection.DiscoverSchema(startPath, cancellationToken).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    var containerCount = result.Value?.Count() ?? 0;
                    _logger.LogInformation(
                        "Schema discovery completed successfully for connection {ConnectionName}. Found {ContainerCount} containers",
                        connectionName,
                        containerCount);
                }
                else
                {
                    _logger.LogWarning(
                        "Schema discovery failed for connection {ConnectionName}: {ErrorMessage}",
                        connectionName,
                        result.Message);
                }

                return result;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Schema discovery failed for connection {0}: {1}",
                    connectionName,
                    ex.Message);

                _logger.LogError(ex,
                    "Exception occurred during schema discovery for connection {ConnectionName}",
                    connectionName);

                return FdwResult<IEnumerable<DataContainer>>.Failure(errorMessage);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IDictionary<string, object>>> GetConnectionsMetadata(CancellationToken cancellationToken = default)
    {
        using (_logger.BeginScope(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Operation"] = "GetConnectionsMetadata"
        }))
        {
            _logger.LogDebug("Retrieving metadata for all connections");

            try
            {
                var metadata = new Dictionary<string, object>(StringComparer.Ordinal);
                var connectionTasks = new List<Task>();

                foreach (var kvp in _connections)
                {
                    var connectionName = kvp.Key;
                    var connection = kvp.Value;

                    connectionTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // Try to cast to the expected interface
                            if (connection is IExternalDataConnection<IExternalConnectionConfiguration> dataConnection)
                            {
                                var connectionInfo = await dataConnection.GetConnectionInfo(cancellationToken).ConfigureAwait(false);
                                if (connectionInfo.IsSuccess)
                                {
                                    lock (metadata)
                                    {
                                        metadata[connectionName] = connectionInfo.Value!;
                                    }
                                }
                                else
                                {
                                    lock (metadata)
                                    {
                                        metadata[connectionName] = new Dictionary<string, object>(StringComparer.Ordinal)
                                        {
                                            ["Error"] = connectionInfo.Message ?? "Unknown error",
                                            ["Available"] = false
                                        };
                                    }
                                }
                            }
                            else
                            {
                                lock (metadata)
                                {
                                    metadata[connectionName] = new Dictionary<string, object>(StringComparer.Ordinal)
                                    {
                                        ["Error"] = "Connection does not implement required interface",
                                        ["Available"] = false
                                    };
                                }
                            }
                        }
                        catch (Exception ex) when (ex is not OutOfMemoryException)
                        {
                            _logger.LogWarning(ex,
                                "Failed to get metadata for connection {ConnectionName}",
                                connectionName);

                            lock (metadata)
                            {
                                metadata[connectionName] = new Dictionary<string, object>(StringComparer.Ordinal)
                                {
                                    ["Error"] = ex.Message,
                                    ["Available"] = false
                                };
                            }
                        }
                    }, cancellationToken));
                }

                await Task.WhenAll(connectionTasks).ConfigureAwait(false);

                _logger.LogInformation(
                    "Successfully retrieved metadata for {ConnectionCount} connections",
                    metadata.Count);

                return FdwResult<IDictionary<string, object>>.Success(metadata);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to retrieve connections metadata: {0}",
                    ex.Message);

                _logger.LogError(ex, "Exception occurred while retrieving connections metadata");

                return FdwResult<IDictionary<string, object>>.Failure(errorMessage);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<bool>> IsConnectionAvailable(string connectionName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
        {
            const string errorMessage = "Connection name cannot be null or empty";
            _logger.LogError("IsConnectionAvailable called with null or empty connection name");
            return FdwResult<bool>.Failure(errorMessage);
        }

        using (_logger.BeginScope(new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["Operation"] = "IsConnectionAvailable",
            ["ConnectionName"] = connectionName
        }))
        {
            _logger.LogDebug("Checking availability for connection {ConnectionName}", connectionName);

            try
            {
                var connectionObj = GetConnection(connectionName);
                if (connectionObj == null)
                {
                    _logger.LogDebug(
                        "Connection {ConnectionName} not found in registry",
                        connectionName);

                    return FdwResult<bool>.Success(false);
                }

                // Try to cast to the expected interface
                if (connectionObj is not IExternalDataConnection<IExternalConnectionConfiguration> connection)
                {
                    _logger.LogDebug(
                        "Connection {ConnectionName} does not implement required interface",
                        connectionName);

                    return FdwResult<bool>.Success(false);
                }

                var result = await connection.TestConnection(cancellationToken).ConfigureAwait(false);

                var isAvailable = result.IsSuccess && result.Value;
                
                _logger.LogDebug(
                    "Connection {ConnectionName} availability check completed: {IsAvailable}",
                    connectionName,
                    isAvailable);

                return FdwResult<bool>.Success(isAvailable);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                var errorMessage = string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to check availability for connection {0}: {1}",
                    connectionName,
                    ex.Message);

                _logger.LogError(ex,
                    "Exception occurred while checking availability for connection {ConnectionName}",
                    connectionName);

                return FdwResult<bool>.Failure(errorMessage);
            }
        }
    }

    /// <summary>
    /// Registers a new named connection with the provider.
    /// </summary>
    /// <param name="name">The unique name for the connection.</param>
    /// <param name="connection">The connection instance to register.</param>
    /// <returns>True if the connection was registered successfully; false if a connection with the same name already exists.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when connection is null.</exception>
    public bool RegisterConnection<TConfiguration>(string name, IExternalDataConnection<TConfiguration> connection)
        where TConfiguration : IExternalConnectionConfiguration
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Connection name cannot be null or empty.", nameof(name));
        
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        var registered = _connections.TryAdd(name, connection);
        
        if (registered)
        {
            _logger.LogInformation(
                "Successfully registered connection {ConnectionName}",
                name);
        }
        else
        {
            _logger.LogWarning(
                "Failed to register connection {ConnectionName} - name already exists",
                name);
        }

        return registered;
    }

    /// <summary>
    /// Unregisters a connection from the provider.
    /// </summary>
    /// <param name="name">The name of the connection to unregister.</param>
    /// <returns>True if the connection was unregistered successfully; false if the connection was not found.</returns>
    /// <exception cref="ArgumentException">Thrown when name is null or empty.</exception>
    public bool UnregisterConnection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Connection name cannot be null or empty.", nameof(name));

        var removed = _connections.TryRemove(name, out _);
        
        if (removed)
        {
            _logger.LogInformation(
                "Successfully unregistered connection {ConnectionName}",
                name);
        }
        else
        {
            _logger.LogWarning(
                "Failed to unregister connection {ConnectionName} - not found",
                name);
        }

        return removed;
    }

    /// <summary>
    /// Gets the names of all registered connections.
    /// </summary>
    /// <returns>A collection of all registered connection names.</returns>
    public IEnumerable<string> GetConnectionNames()
    {
        return _connections.Keys.ToList();
    }

    /// <summary>
    /// Gets the count of registered connections.
    /// </summary>
    public int ConnectionCount => _connections.Count;
}
