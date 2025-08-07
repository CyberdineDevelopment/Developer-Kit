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

    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionBase{TConfiguration, TCommand, TConnection}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configurations">The configuration registry.</param>
    protected ConnectionBase(
        ILogger<TConnection>? logger,
        IConfigurationRegistry<TConfiguration> configurations)
        : base(logger, configurations)
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
            return Logger.FailureWithLog("Invalid connection credentials provided");
        }

        await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
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
                    return Logger.FailureWithLog(result.Message ?? "Connection attempt failed");
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return Logger.FailureWithLog($"Connection timeout to {connectionString} after {ConnectionTimeoutSeconds} seconds");
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
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
                return Logger.FailureWithLog(result.Message ?? "Disconnect failed");
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            return Logger.FailureWithLog("Connection attempt failed");
        }

        try
        {
            return await OnTestConnectionAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return Logger.FailureWithLog(ex, "Connection attempt failed");
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
            return Logger.FailureWithLog<T>("Connection attempt failed");
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
    public async Task<IFdwResult> Connect(string connectionString, CancellationToken cancellationToken = default)
    {
        return await ConnectAsync(connectionString, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> Disconnect(CancellationToken cancellationToken = default)
    {
        return await DisconnectAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> Test(CancellationToken cancellationToken = default)
    {
        return await TestConnectionAsync(cancellationToken).ConfigureAwait(false);
    }

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
            _connectionLock?.Dispose();
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
        _connectionLock?.Dispose();
    }
}