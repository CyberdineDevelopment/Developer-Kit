using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Connections;

/// <summary>
/// Defines a wrapper around a connection
/// </summary>
public interface IFdwConnection : IDisposable, IAsyncDisposable
{
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
    Task<IFdwResult> Execute(ICommand commandToBeExecuted, CancellationToken cancellationToken);

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
public interface IFdwConnection<TConnection> : IFdwConnection
where TConnection : IExternalConnection
{
    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="commandToBeExecuted">An object that contains the commandToBeExecuted to be executed in the external provider's native syntax.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the connection is valid.</returns>
    new Task<IFdwResult> Execute(ICommand commandToBeExecuted, CancellationToken cancellationToken);

}

/// <summary>
/// Defines a connection that can be established, tested, and disconnected.
/// </summary>
/// <typeparam name="TConnection">The type of the connection</typeparam>
/// <typeparam name="TCommand">The type of the command expected by the connection</typeparam>
public interface IFdwConnection<TConnection, TCommand> : IFdwConnection<TConnection>
where  TConnection : IExternalConnection<TCommand>
where TCommand : ICommand 
{
    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="commandToBeExecuted">An object that contains the commandToBeExecuted to be executed in the external provider's native syntax.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating whether the connection is valid.</returns>
    new Task<IFdwResult> Execute(ICommand commandToBeExecuted, CancellationToken cancellationToken);
}