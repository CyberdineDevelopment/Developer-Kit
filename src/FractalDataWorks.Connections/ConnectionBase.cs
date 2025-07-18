using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Commands;
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
public abstract class ConnectionBase<TConfiguration, TCommand, TConnection> 
    : ServiceBase<TConfiguration, TCommand, TConnection>, IFdwConnection
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
    public async Task<FdwResult> ConnectAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return FdwResult.Failure(ConnectionMessages.InvalidCredentials.Format("connection string"));
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
            {
                Logger.LogWarning("Already connected to {ConnectionString}", ConnectionString);
                return FdwResult.Success();
            }

            Logger.LogInformation("Connecting to {ConnectionString}", connectionString);
            ConnectionString = connectionString;

            // Set up timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(ConnectionTimeoutSeconds));

            try
            {
                var result = await OnConnectAsync(connectionString, cts.Token);
                if (result.IsSuccess)
                {
                    IsConnected = true;
                    ConnectedAt = DateTimeOffset.UtcNow;
                    DisconnectedAt = null;
                    Logger.LogInformation("Successfully connected to {ConnectionString}", connectionString);
                }
                else
                {
                    Logger.LogError("Failed to connect: {Error}", result.Error);
                }
                return result;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                var message = ConnectionMessages.ConnectionTimeout.Format(connectionString, ConnectionTimeoutSeconds);
                Logger.LogError(message);
                return FdwResult.Failure(ConnectionMessages.ConnectionTimeout);
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<FdwResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (!IsConnected)
            {
                Logger.LogWarning("Not connected");
                return FdwResult.Success();
            }

            Logger.LogInformation("Disconnecting from {ConnectionString}", ConnectionString);
            
            var result = await OnDisconnectAsync(cancellationToken);
            
            IsConnected = false;
            DisconnectedAt = DateTimeOffset.UtcNow;
            
            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully disconnected");
            }
            else
            {
                Logger.LogError("Error during disconnect: {Error}", result.Error);
            }
            
            return result;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<FdwResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            return FdwResult.Failure(ConnectionMessages.ConnectionFailed.Format("Not connected"));
        }

        try
        {
            return await OnTestConnectionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Connection test failed");
            return FdwResult.Failure(ConnectionMessages.ConnectionFailed.Format(ex.Message));
        }
    }

    /// <summary>
    /// When overridden in a derived class, performs the actual connection.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection result.</returns>
    protected abstract Task<FdwResult> OnConnectAsync(string connectionString, CancellationToken cancellationToken);

    /// <summary>
    /// When overridden in a derived class, performs the actual disconnection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The disconnection result.</returns>
    protected abstract Task<FdwResult> OnDisconnectAsync(CancellationToken cancellationToken);

    /// <summary>
    /// When overridden in a derived class, tests the connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The test result.</returns>
    protected abstract Task<FdwResult> OnTestConnectionAsync(CancellationToken cancellationToken);

    /// <inheritdoc/>
    protected override async Task<FdwResult<T>> ExecuteCore<T>(TCommand command)
    {
        if (!IsConnected)
        {
            return FdwResult<T>.Failure(ConnectionMessages.ConnectionFailed.Format("Not connected"));
        }

        // Derived classes implement specific command execution
        return await OnExecuteCommandAsync<T>(command);
    }

    /// <summary>
    /// When overridden in a derived class, executes a command on the connection.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <returns>The execution result.</returns>
    protected abstract Task<FdwResult<T>> OnExecuteCommandAsync<T>(TCommand command);

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
                DisconnectAsync().GetAwaiter().GetResult();
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