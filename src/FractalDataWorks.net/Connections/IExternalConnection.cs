using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Connections;

/// <summary>
/// Defines a connection that can be established, tested, and disconnected.
/// </summary>
public interface IExternalConnection : IDisposable, IAsyncDisposable
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
    /// Establishes a connection.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> Connect(string connectionString, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<IFdwResult> Disconnect(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the connection is valid.</returns>
    Task<IFdwResult> Test(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="commandToBeExecuted">An object that contains the command to be executed in the external provider's native syntax.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the connection is valid.</returns>
    Task<IFdwResult> Execute(ICommand commandToBeExecuted,CancellationToken cancellationToken);
}

/// <summary>
/// Defines a connection that can be established, tested, and disconnected.
/// </summary>
/// <typeparam name="TCommand">The type of the native command.</typeparam>
public interface IExternalConnection<in TCommand> : IExternalConnection
where TCommand : ICommand
{
    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="commandToBeExecuted">An object that contains the commandToBeExecuted to be executed in the external provider's native syntax.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the connection is valid.</returns>
    Task<IFdwResult> Execute(TCommand commandToBeExecuted, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <typeparam name="T">The type of result expected</typeparam>
    /// <param name="commandToBeExecuted"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A result of the {T}</returns>
    Task<IFdwResult<T>> Execute<T>(ICommand<T> commandToBeExecuted, CancellationToken cancellationToken);

}

/// <summary>
/// Defines a connection that can be established, tested, and disconnected.
/// </summary>
/// <typeparam name="TConnection">The type of the connection</typeparam>
/// <typeparam name="TCommand">The type of the native command.</typeparam>
public interface IExternalConnection<TConnection,in TCommand> : IExternalConnection<TCommand>
where TCommand : ICommand
{

}