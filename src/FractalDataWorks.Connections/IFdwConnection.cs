using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Connections;

/// <summary>
/// Defines a connection that can be established, tested, and disconnected.
/// </summary>
public interface IFdwConnection : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the unique identifier for this connection.
    /// </summary>
    Guid ConnectionId { get; }

    /// <summary>
    /// Gets a value indicating whether this connection is currently connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the timestamp when this connection was established.
    /// </summary>
    DateTimeOffset? ConnectedAt { get; }

    /// <summary>
    /// Gets the timestamp when this connection was disconnected.
    /// </summary>
    DateTimeOffset? DisconnectedAt { get; }

    /// <summary>
    /// Gets the connection string used for this connection.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Establishes a connection asynchronously.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> ConnectAsync(string connectionString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the connection is valid.</returns>
    Task<IFdwResult> TestConnectionAsync(CancellationToken cancellationToken = default);
}