using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Configuration;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Connections.Messages;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Connections;

/// <summary>
/// Base class for connection providers that inherit all service functionality.
/// </summary>
/// <typeparam name="TConfiguration">The configuration type for this connection.</typeparam>
/// <typeparam name="TCommand">The command type for this connection.</typeparam>
/// <typeparam name="TConnection">The concrete connection type for logging category.</typeparam>
public abstract class ConnectionBase<TCommand,TConfiguration, TConnection> 
    : ServiceBase<TConfiguration, TCommand, TConnection>, IExternalConnection
    where TConfiguration : ConfigurationBase<TConfiguration>, new()
    where TCommand : ICommand
    where TConnection : class
{
    private static readonly Action<ILogger, string, Exception?> _logAlreadyConnected =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, "AlreadyConnected"),
            "Already connected to {ConnectionString}");

    private static readonly Action<ILogger, string, Exception?> _logConnecting =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, "Connecting"),
            "Connecting to {ConnectionString}");

    private static readonly Action<ILogger, string, Exception?> _logConnected =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(3, "Connected"),
            "Successfully connected to {ConnectionString}");

    private static readonly Action<ILogger, string, Exception?> _logConnectionError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(4, "ConnectionError"),
            "Failed to connect: {Error}");

    private static readonly Action<ILogger, string, Exception?> _logConnectionTimeoutError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(5, "ConnectionTimeout"),
            "{Message}");

    private static readonly Action<ILogger, Exception?> _logNotConnected =
        LoggerMessage.Define(
            LogLevel.Warning,
            new EventId(6, "NotConnected"),
            "Not connected");

    private static readonly Action<ILogger, string, Exception?> _logDisconnecting =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(7, "Disconnecting"),
            "Disconnecting from {ConnectionString}");

    private static readonly Action<ILogger, Exception?> _logDisconnected =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(8, "Disconnected"),
            "Successfully disconnected");

    private static readonly Action<ILogger, string, Exception?> _logDisconnectError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(9, "DisconnectError"),
            "Error during disconnect: {Error}");

    private static readonly Action<ILogger, Exception?> _logConnectionTestFailed =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(10, "ConnectionTestFailed"),
            "Connection test failed");

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
            return FdwResult.Failure(ConnectionMessages.InvalidCredentials);
        }

        await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsConnected)
            {
                _logAlreadyConnected(Logger, ConnectionString, null);
                return FdwResult.Success();
            }

            _logConnecting(Logger, connectionString, null);
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
                    _logConnected(Logger, connectionString, null);
                }
                else
                {
                    _logConnectionError(Logger, result.Message?.Message ?? "Unknown error", null);
                }
                return result;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var message = ConnectionMessages.ConnectionTimeout.Format(connectionString, ConnectionTimeoutSeconds);
                _logConnectionTimeoutError(Logger, message, null);
                return FdwResult.Failure(ConnectionMessages.ConnectionTimeout);
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
                _logNotConnected(Logger, null);
                return FdwResult.Success();
            }

            _logDisconnecting(Logger, ConnectionString, null);
            
            var result = await OnDisconnectAsync(cancellationToken).ConfigureAwait(false);
            
            IsConnected = false;
            DisconnectedAt = DateTimeOffset.UtcNow;
            
            if (result.IsSuccess)
            {
                _logDisconnected(Logger, null);
            }
            else
            {
                _logDisconnectError(Logger, result.Message?.Message ?? "Unknown error", null);
            }
            
            return result;
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
            return FdwResult.Failure(ConnectionMessages.ConnectionFailed);
        }

        try
        {
            return await OnTestConnectionAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logConnectionTestFailed(Logger, ex);
            return FdwResult.Failure(ConnectionMessages.ConnectionFailed);
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

    /// <inheritdoc/>
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(TCommand command)
    {
        if (!IsConnected)
        {
            return FdwResult<T>.Failure(ConnectionMessages.ConnectionFailed);
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