using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
// Removed old messages namespace
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections;

/// <summary>
/// Base class for connection providers that inherit all service functionality.
/// </summary>
/// <typeparam name="TConfiguration">The configuration type for this connection.</typeparam>
/// <typeparam name="TCommand">The command type for this connection.</typeparam>
/// <typeparam name="TConnection">The concrete connection type for logging category.</typeparam>
public abstract class ConnectionBase<TCommand,TConfiguration, TConnection> 
    : ServiceBase<TCommand, TConfiguration, TConnection>, IExternalConnection
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
    where TCommand : ICommand
    where TConnection : class
{
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionBase{TConfiguration, TCommand, TConnection}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration instance.</param>
    protected ConnectionBase(
        ILogger<TConnection>? logger,
        TConfiguration configuration)
        : base(logger, configuration)
    {
        ConnectionId = Guid.NewGuid();
    }

    /// <inheritdoc/>
    public Guid ConnectionId { get; }

    /// <inheritdoc/>
    public bool IsConnected { get; protected set; }

    /// <inheritdoc/>
    public DateTimeOffset? ConnectedAt { get; protected set; }

    /// <inheritdoc/>
    public DateTimeOffset? DisconnectedAt { get; protected set; }

    /// <inheritdoc/>
    public string ConnectionString { get; protected set; } = string.Empty;

    /// <summary>
    /// Gets the connection timeout in seconds.
    /// </summary>
    public virtual int ConnectionTimeoutSeconds => 30;

    /// <inheritdoc/>
    public async Task<IFdwResult> ConnectAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            ConnectionBaseLog.InvalidCredentials(Logger);
            return FdwResult.Failure("Invalid connection credentials provided");
        }

        try
        {
            if (IsConnected)
            {
                ConnectionBaseLog.AlreadyConnected(Logger, ConnectionString);
                return FdwResult.Success();
            }

            ConnectionBaseLog.Connecting(Logger, connectionString);
            ConnectionString = connectionString;

            // Set up timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(ConnectionTimeoutSeconds));

            try
            {
                var result = await OnConnectAsync(connectionString, cts.Token).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    IsConnected = true;
                    ConnectedAt = DateTimeOffset.UtcNow;
                    DisconnectedAt = null;
                    ConnectionBaseLog.Connected(Logger, connectionString);
                    return FdwResult.Success();
                }
                else
                {
                    var message = result.Message ?? "Connection attempt failed";
                    ConnectionBaseLog.ConnectionFailed(Logger, message);
                    return FdwResult.Failure(message);
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                ConnectionBaseLog.ConnectionTimeout(Logger, connectionString, ConnectionTimeoutSeconds);
                return FdwResult.Failure($"Connection timeout to {connectionString} after {ConnectionTimeoutSeconds} seconds");
            }
        }
        finally
        {
            // Removed semaphore - connections should handle their own thread safety if needed
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsConnected)
            {
                ConnectionBaseLog.NotConnected(Logger);
                return FdwResult.Success();
            }

            ConnectionBaseLog.Disconnecting(Logger, ConnectionString);
            
            var result = await OnDisconnectAsync(cancellationToken).ConfigureAwait(false);
            
            IsConnected = false;
            DisconnectedAt = DateTimeOffset.UtcNow;
            
            if (result.IsSuccess)
            {
                ConnectionBaseLog.Disconnected(Logger);
                return FdwResult.Success();
            }
            else
            {
                var message = result.Message ?? "Disconnect failed";
                ConnectionBaseLog.ConnectionFailed(Logger, message);
                return FdwResult.Failure(message);
            }
        }
        finally
        {
            // Removed semaphore - connections should handle their own thread safety if needed
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            ConnectionBaseLog.ConnectionFailed(Logger, "Connection attempt failed");
            return FdwResult.Failure("Connection attempt failed");
        }

        try
        {
            return await OnTestConnectionAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ConnectionBaseLog.ConnectionAttemptFailed(Logger, ex);
            return FdwResult.Failure("Connection attempt failed");
        }
    }

    /// <summary>
    /// When overridden in a derived class, performs the actual connection.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection result.</returns>
    protected abstract Task<IFdwResult> OnConnectAsync(string connectionString, CancellationToken cancellationToken);

    /// <summary>
    /// When overridden in a derived class, performs the actual disconnection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The disconnection result.</returns>
    protected abstract Task<IFdwResult> OnDisconnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// When overridden in a derived class, tests the connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The test result.</returns>
    protected abstract Task<IFdwResult> OnTestConnectionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Executes a command through the connection.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>The execution result.</returns>
    protected async Task<IFdwResult<T>> ExecuteCommandAsync<T>(TCommand command)
    {
        if (!IsConnected)
        {
            ConnectionBaseLog.ConnectionFailed(Logger, "Connection attempt failed");
            return FdwResult<T>.Failure("Connection attempt failed");
        }

        // Derived classes implement specific command execution
        return await OnExecuteCommandAsync<T>(command).ConfigureAwait(false);
    }

    /// <summary>
    /// When overridden in a derived class, executes a command on the connection.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>The execution result.</returns>
    protected abstract Task<IFdwResult<T>> OnExecuteCommandAsync<T>(TCommand command);

    #region IExternalConnection Implementation

    /// <inheritdoc/>
    public Task<IFdwResult> Connect(string connectionString, CancellationToken cancellationToken = default)
        => ConnectAsync(connectionString, cancellationToken);

    /// <inheritdoc/>
    public Task<IFdwResult> Disconnect(CancellationToken cancellationToken = default)
        => DisconnectAsync(cancellationToken);

    /// <inheritdoc/>
    public Task<IFdwResult> Test(CancellationToken cancellationToken = default)
        => TestConnectionAsync(cancellationToken);

    #endregion

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            if (IsConnected)
            {
                // Best effort disconnect - don't block on disposal
                _ = Task.Run(async () => await DisconnectAsync().ConfigureAwait(false));
            }
            // Semaphore removed - no disposal needed
        }

        _disposed = true;
    }

    /// <summary>
    /// Asynchronously releases resources.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (IsConnected)
        {
            await DisconnectAsync().ConfigureAwait(false);
        }
    }
}