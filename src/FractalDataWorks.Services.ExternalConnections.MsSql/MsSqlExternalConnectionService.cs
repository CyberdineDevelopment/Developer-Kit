using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DataProvider.Abstractions.Models;
using FractalDataWorks.Services.ExternalConnections.Abstractions;
using FractalDataWorks.Services.ExternalConnections.Abstractions.Commands;
using FractalDataWorks.Services.ExternalConnections.MsSql.Commands;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.ExternalConnections.MsSql;

/// <summary>
/// SQL Server implementation of external connection service.
/// This service handles MsSql connection commands and manages connection instances.
/// </summary>
public sealed class MsSqlExternalConnectionService 
    : ExternalConnectionServiceBase<IExternalConnectionCommand, MsSqlConfiguration, MsSqlExternalConnectionService>, IFdwService
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, MsSqlExternalConnection> _connections;
    private readonly string _serviceId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlExternalConnectionService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="loggerFactory">The logger factory for creating connection loggers.</param>
    /// <param name="configuration">The MsSql service configuration.</param>
    public MsSqlExternalConnectionService(
        ILogger<MsSqlExternalConnectionService> logger, 
        ILoggerFactory loggerFactory,
        MsSqlConfiguration configuration) 
        : base(logger, configuration)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _connections = new Dictionary<string, MsSqlExternalConnection>(StringComparer.Ordinal);
        _serviceId = Guid.NewGuid().ToString("N");
    }

    /// <inheritdoc/>
    public override string Id => _serviceId;

    /// <inheritdoc/>
    public override string ServiceType => "MsSql External Connection Service";

    /// <inheritdoc/>
    public override bool IsAvailable => _connections.Count > 0;

    /// <inheritdoc/>
    protected override async Task<IFdwResult<T>> ExecuteCore<T>(IExternalConnectionCommand command)
    {
        var result = await ExecuteInternal(command, CancellationToken.None).ConfigureAwait(false);
        
        if (result.IsSuccess && result.Value is T typedValue)
        {
            return FdwResult<T>.Success(typedValue);
        }
        
        if (result.IsSuccess)
        {
            // Try to convert the result to the expected type
            try
            {
                var convertedValue = (T)(object)result.Value!;
                return FdwResult<T>.Success(convertedValue);
            }
            catch (InvalidCastException)
            {
                return FdwResult<T>.Failure($"Unable to convert result to type {typeof(T).Name}");
            }
        }
        
        return FdwResult<T>.Failure(result.Message ?? "Command execution failed");
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult<TOut>> Execute<TOut>(IExternalConnectionCommand command, CancellationToken cancellationToken)
    {
        var result = await ExecuteInternal(command, cancellationToken).ConfigureAwait(false);
        
        if (result.IsSuccess && result.Value is TOut typedValue)
        {
            return FdwResult<TOut>.Success(typedValue);
        }
        
        if (result.IsSuccess)
        {
            // Try to convert the result to the expected type
            try
            {
                var convertedValue = (TOut)(object)result.Value!;
                return FdwResult<TOut>.Success(convertedValue);
            }
            catch (InvalidCastException)
            {
                return FdwResult<TOut>.Failure($"Unable to convert result to type {typeof(TOut).Name}");
            }
        }
        
        return FdwResult<TOut>.Failure(result.Message ?? "Command execution failed");
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult> Execute(IExternalConnectionCommand command, CancellationToken cancellationToken)
    {
        var result = await ExecuteInternal(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? FdwResult.Success() : FdwResult.Failure(result.Message!);
    }

    /// <summary>
    /// Internal method for executing external connection commands.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The command execution result.</returns>
    private async Task<IFdwResult<object>> ExecuteInternal(IExternalConnectionCommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        Logger.LogDebug("Executing {CommandType} command", command.GetType().Name);

        try
        {
            return command switch
            {
                // Note: Test connection functionality is handled via management commands with TestConnection operation
                IExternalConnectionDiscoveryCommand discoveryCommand => await HandleConnectionDiscoveryCommand(discoveryCommand, cancellationToken).ConfigureAwait(false),
                IExternalConnectionCreateCommand createCommand => await HandleConnectionCreateCommand(createCommand, cancellationToken).ConfigureAwait(false),
                IExternalConnectionManagementCommand mgmtCommand => await HandleConnectionManagementCommand(mgmtCommand, cancellationToken).ConfigureAwait(false),
                _ => FdwResult<object>.Failure($"Unsupported command type: {command.GetType().Name}")
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute {CommandType} command", command.GetType().Name);
            return FdwResult<object>.Failure($"Command execution failed: {ex.Message}");
        }
    }

    // Note: Connection testing is now handled via management commands with TestConnection operation

    private async Task<IFdwResult<object>> HandleConnectionDiscoveryCommand(IExternalConnectionDiscoveryCommand command, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Discovering schema for connection {ConnectionName}", command.ConnectionName);
        
        if (!_connections.TryGetValue(command.ConnectionName, out var connection))
        {
            return FdwResult<object>.Failure($"Connection '{command.ConnectionName}' not found");
        }

        var result = await connection.DiscoverSchema().ConfigureAwait(false);
        return result.IsSuccess 
            ? FdwResult<object>.Success(result.Value?.ToArray() ?? Array.Empty<DataContainer>())
            : FdwResult<object>.Failure(result.Message ?? "Schema discovery failed");
    }

    private async Task<IFdwResult<object>> HandleConnectionCreateCommand(IExternalConnectionCreateCommand command, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Creating connection {ConnectionName}", command.ConnectionName);
        
        if (_connections.ContainsKey(command.ConnectionName))
        {
            return FdwResult<object>.Failure($"Connection '{command.ConnectionName}' already exists");
        }

        if (command.ConnectionConfiguration is not MsSqlConfiguration msSqlConfig)
        {
            return FdwResult<object>.Failure("Invalid configuration type for MsSql connection");
        }

        var connection = new MsSqlExternalConnection(
            _loggerFactory.CreateLogger<MsSqlExternalConnection>(),
            msSqlConfig);

        // No need to initialize in stateless design
        var initResult = FdwResult.Success();
        if (!initResult.IsSuccess)
        {
            connection.Dispose();
            return FdwResult<object>.Failure($"Failed to initialize connection: {initResult.Message}");
        }

        _connections[command.ConnectionName] = connection;
        Logger.LogInformation("Successfully created connection {ConnectionName}", command.ConnectionName);
        
        return FdwResult<object>.Success($"Connection '{command.ConnectionName}' created successfully");
    }

    private async Task<IFdwResult<object>> HandleConnectionManagementCommand(IExternalConnectionManagementCommand command, CancellationToken cancellationToken)
    {
        return command.Operation switch
        {
            ConnectionManagementOperation.ListConnections => await HandleListConnections(cancellationToken).ConfigureAwait(false),
            ConnectionManagementOperation.RemoveConnection => await HandleRemoveConnection(command.ConnectionName!, cancellationToken).ConfigureAwait(false),
            ConnectionManagementOperation.GetConnectionMetadata => await HandleGetConnectionMetadata(command.ConnectionName!, cancellationToken).ConfigureAwait(false),
            ConnectionManagementOperation.RefreshConnectionStatus => await HandleRefreshConnectionStatus(command.ConnectionName!, cancellationToken).ConfigureAwait(false),
            ConnectionManagementOperation.TestConnection => await HandleTestConnection(command.ConnectionName!, cancellationToken).ConfigureAwait(false),
            _ => FdwResult<object>.Failure($"Unsupported management operation: {command.Operation}")
        };
    }

    private async Task<IFdwResult<object>> HandleListConnections(CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // Make async
        
        var connectionList = _connections.Keys.ToArray();
        Logger.LogDebug("Listed {ConnectionCount} connections", connectionList.Length);
        
        return FdwResult<object>.Success(connectionList);
    }

    private async Task<IFdwResult<object>> HandleRemoveConnection(string connectionName, CancellationToken cancellationToken)
    {
        await Task.CompletedTask; // Make async
        
        if (!_connections.TryGetValue(connectionName, out var connection))
        {
            return FdwResult<object>.Failure($"Connection '{connectionName}' not found");
        }

        connection.Dispose();
        _connections.Remove(connectionName);
        
        Logger.LogInformation("Removed connection {ConnectionName}", connectionName);
        return FdwResult<object>.Success($"Connection '{connectionName}' removed successfully");
    }

    private async Task<IFdwResult<object>> HandleGetConnectionMetadata(string connectionName, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(connectionName, out var connection))
        {
            return FdwResult<object>.Failure($"Connection '{connectionName}' not found");
        }

        var result = await connection.GetMetadataAsync().ConfigureAwait(false);
        return result.IsSuccess 
            ? FdwResult<object>.Success(result.Value!)
            : FdwResult<object>.Failure(result.Message ?? "Failed to get connection metadata");
    }

    private async Task<IFdwResult<object>> HandleRefreshConnectionStatus(string connectionName, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(connectionName, out var connection))
        {
            return FdwResult<object>.Failure($"Connection '{connectionName}' not found");
        }

        var testResult = await connection.TestConnectionAsync().ConfigureAwait(false);
        var status = new
        {
            ConnectionName = connectionName,
            IsAvailable = testResult.IsSuccess,
            State = connection.State.ToString(),
            Message = testResult.Message
        };

        Logger.LogDebug("Refreshed status for connection {ConnectionName}: {IsAvailable}", connectionName, testResult.IsSuccess);
        return FdwResult<object>.Success(status);
    }

    private async Task<IFdwResult<object>> HandleTestConnection(string connectionName, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(connectionName, out var connection))
        {
            return FdwResult<object>.Failure($"Connection '{connectionName}' not found");
        }

        var result = await connection.TestConnectionAsync().ConfigureAwait(false);
        return result.IsSuccess 
            ? FdwResult<object>.Success(true)
            : FdwResult<object>.Failure(result.Message ?? "Connection test failed");
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var connection in _connections.Values)
            {
                try
                {
                    connection.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Exception occurred while disposing connection");
                }
            }
            _connections.Clear();
        }

        // base.Dispose(disposing); // ServiceBase doesn't have a virtual Dispose method
    }
}
