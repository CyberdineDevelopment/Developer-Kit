using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.DataProvider.Abstractions.Commands;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;

namespace FractalDataWorks.Services.DataProvider.Services;

/// <summary>
/// Interface for external data connections that can execute commands and provide schema information.
/// </summary>
/// <remarks>
/// This interface defines the contract for individual connection implementations that handle
/// specific data store types (SQL databases, file systems, REST APIs, etc.).
/// </remarks>
public interface IExternalDataConnection
{
    /// <summary>
    /// Executes a data command against this connection.
    /// </summary>
    /// <typeparam name="T">The expected return type of the command execution.</typeparam>
    /// <param name="command">The data command to execute.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult&lt;T&gt; with the command execution result or error information.
    /// </returns>
    Task<IFdwResult<T>> Execute<T>(DataCommandBase command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers the schema structure starting from an optional path.
    /// </summary>
    /// <param name="startPath">Optional starting path for schema discovery.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult with a collection of discovered data containers.
    /// </returns>
    Task<IFdwResult<IEnumerable<DataContainer>>> DiscoverSchema(DataPath? startPath = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests if this connection is available and operational.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult&lt;bool&gt; indicating whether the connection is available.
    /// </returns>
    Task<IFdwResult<bool>> TestConnection(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets metadata information about this connection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// an IFdwResult with connection metadata.
    /// </returns>
    Task<IFdwResult<IDictionary<string, object>>> GetConnectionInfo(CancellationToken cancellationToken = default);
}