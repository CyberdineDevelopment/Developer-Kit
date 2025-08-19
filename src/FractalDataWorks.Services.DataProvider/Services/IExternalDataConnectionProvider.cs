using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Services;

/// <summary>
/// Interface for external data connection providers that manage named connections to external data stores.
/// </summary>
/// <remarks>
/// This interface will be implemented later as part of the external connections infrastructure.
/// It represents the bridge between the DataProvider service and actual external data connections
/// such as SQL databases, file systems, REST APIs, and other data sources.
/// 
/// Each provider manages a collection of named connections and routes commands to the appropriate
/// connection based on the command's ConnectionName property.
/// </remarks>
public interface IExternalDataConnectionProvider
{
    /// <summary>
    /// Executes a data command against the specified named connection.
    /// </summary>
    /// <typeparam name="T">The expected return type of the command execution.</typeparam>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult&lt;T&gt; with the command execution result or error information.
    /// </returns>
    /// <remarks>
    /// This method will:
    /// 1. Resolve the named connection from the command.ConnectionName
    /// 2. Translate the provider-agnostic command to the target system's native format
    /// 3. Execute the command against the external data store
    /// 4. Return the typed result or error information
    /// </remarks>
    Task<IFdwResult<T>> ExecuteCommand<T>(DataCommandBase command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers the schema structure of a named connection starting from an optional path.
    /// </summary>
    /// <param name="connectionName">The name of the connection to discover schema for.</param>
    /// <param name="startPath">Optional starting path for schema discovery.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult with a collection of discovered data containers.
    /// </returns>
    Task<IFdwResult<IEnumerable<DataContainer>>> DiscoverConnectionSchema(
        string connectionName, 
        DataPath? startPath = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about all available named connections.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult with a dictionary of connection names and their metadata.
    /// </returns>
    Task<IFdwResult<IDictionary<string, object>>> GetConnectionsMetadata(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a named connection is available and operational.
    /// </summary>
    /// <param name="connectionName">The name of the connection to check.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult&lt;bool&gt; indicating whether the connection is available.
    /// </returns>
    Task<IFdwResult<bool>> IsConnectionAvailable(string connectionName, CancellationToken cancellationToken = default);
}