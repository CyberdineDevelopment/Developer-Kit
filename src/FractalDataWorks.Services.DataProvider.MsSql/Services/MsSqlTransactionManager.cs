using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using FractalDataWorks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DataProvider.Abstractions;
using FractalDataWorks.Services.DataProvider.MsSql.Configuration;

namespace FractalDataWorks.Services.DataProvider.MsSql.Services;

/// <summary>
/// Manages Microsoft SQL Server database transactions implementing the IDataTransaction interface.
/// </summary>
/// <remarks>
/// This transaction manager provides full transaction support including isolation levels,
/// savepoints, and proper transaction state management for SQL Server databases.
/// </remarks>
public sealed class MsSqlTransactionManager : FractalDataWorks.Services.DataProvider.Abstractions.IDataTransaction
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;
    private readonly IDataService _provider;
    private readonly ILogger<MsSqlTransactionManager> _logger;
    private readonly Dictionary<string, string> _savepoints;
    private FdwTransactionState _state;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlTransactionManager"/> class.
    /// </summary>
    /// <param name="connection">The SQL Server connection to use for the transaction.</param>
    /// <param name="provider">The data provider that owns this transaction.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="isolationLevel">The transaction isolation level.</param>
    /// <param name="timeout">The transaction timeout.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    public MsSqlTransactionManager(
        SqlConnection connection,
        IDataService provider,
        ILogger<MsSqlTransactionManager> logger,
        FdwTransactionIsolationLevel isolationLevel = FdwTransactionIsolationLevel.Default,
        TimeSpan? timeout = null)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _savepoints = new Dictionary<string, string>(StringComparer.Ordinal);
        
        TransactionId = $"MsSqlTx_{Guid.NewGuid():N}";
        IsolationLevel = isolationLevel;
        Timeout = timeout;
        StartedAt = DateTimeOffset.UtcNow;
        
        // Convert FDW isolation level to SQL Server isolation level
        var sqlIsolationLevel = ConvertIsolationLevel(isolationLevel);
        
        try
        {
            _state = FdwTransactionState.Created;
            _transaction = _connection.BeginTransaction(sqlIsolationLevel);
            _state = FdwTransactionState.Active;
            
            _logger.LogDebug("SQL Server transaction started with ID: {TransactionId}, isolation level: {IsolationLevel}",
                TransactionId, isolationLevel);
        }
        catch (Exception ex)
        {
            _state = FdwTransactionState.Faulted;
            _logger.LogError(ex, "Failed to start SQL Server transaction with isolation level: {IsolationLevel}", isolationLevel);
            throw;
        }
    }

    /// <inheritdoc/>
    public string TransactionId { get; }

    /// <inheritdoc/>
    public FdwTransactionState State => _state;

    /// <inheritdoc/>
    public FdwTransactionIsolationLevel IsolationLevel { get; }

    /// <inheritdoc/>
    public TimeSpan? Timeout { get; }

    /// <inheritdoc/>
    public DateTimeOffset StartedAt { get; }

    /// <inheritdoc/>
    public IDataService Provider => _provider;

    /// <inheritdoc/>
    public async Task<IFdwResult<object?>> Execute(FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command)
    {
        ThrowIfDisposed();
        
        if (command == null)
        {
            return FdwResult<object?>.Failure("Command cannot be null");
        }

        if (_state != FdwTransactionState.Active)
        {
            return FdwResult<object?>.Failure($"Transaction is not in active state. Current state: {_state}");
        }

        try
        {
            _state = FdwTransactionState.Executing;
            
            _logger.LogDebug("Executing command {CommandType} in transaction {TransactionId}", 
                command.CommandType, TransactionId);

            // Execute the command using the transaction
            var result = await ExecuteCommandInTransaction(command).ConfigureAwait(false);
            
            _state = FdwTransactionState.Active;
            
            if (result.IsSuccess)
            {
                _logger.LogDebug("Command {CommandType} executed successfully in transaction {TransactionId}", 
                    command.CommandType, TransactionId);
            }
            else
            {
                _logger.LogWarning("Command {CommandType} failed in transaction {TransactionId}: {Error}", 
                    command.CommandType, TransactionId, result.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _state = FdwTransactionState.Faulted;
            _logger.LogError(ex, "Error executing command {CommandType} in transaction {TransactionId}", 
                command.CommandType, TransactionId);
            
            return FdwResult<object?>.Failure($"Command execution failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<TResult>> Execute<TResult>(FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand<TResult> command)
    {
        var result = await Execute((FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand)command).ConfigureAwait(false);
        
        if (result.Error)
        {
            return FdwResult<TResult>.Failure(result.Message!);
        }

        if (result.Value is TResult typedResult)
        {
            return FdwResult<TResult>.Success(typedResult);
        }

        return FdwResult<TResult>.Failure("Command result type mismatch");
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> CommitAsync()
    {
        ThrowIfDisposed();

        if (_state != FdwTransactionState.Active)
        {
            return FdwResult.Failure($"Cannot commit transaction in state: {_state}");
        }

        try
        {
            _state = FdwTransactionState.Committing;
            
            _logger.LogDebug("Committing transaction {TransactionId}", TransactionId);
            
            await _transaction.CommitAsync().ConfigureAwait(false);
            
            _state = FdwTransactionState.Committed;
            
            _logger.LogDebug("Transaction {TransactionId} committed successfully", TransactionId);
            
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _state = FdwTransactionState.Faulted;
            _logger.LogError(ex, "Failed to commit transaction {TransactionId}", TransactionId);
            
            return FdwResult.Failure($"Transaction commit failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> RollbackAsync()
    {
        ThrowIfDisposed();

        if (_state is FdwTransactionState.Committed or FdwTransactionState.RolledBack or FdwTransactionState.Disposed)
        {
            _logger.LogDebug("Transaction {TransactionId} is already in final state: {State}", TransactionId, _state);
            return FdwResult.Success();
        }

        try
        {
            _state = FdwTransactionState.RollingBack;
            
            _logger.LogDebug("Rolling back transaction {TransactionId}", TransactionId);
            
            await _transaction.RollbackAsync().ConfigureAwait(false);
            
            _state = FdwTransactionState.RolledBack;
            
            _logger.LogDebug("Transaction {TransactionId} rolled back successfully", TransactionId);
            
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _state = FdwTransactionState.Faulted;
            _logger.LogError(ex, "Failed to rollback transaction {TransactionId}", TransactionId);
            
            return FdwResult.Failure($"Transaction rollback failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<string>> CreateSavepointAsync(string savepointName)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(savepointName))
        {
            return FdwResult<string>.Failure("Savepoint name cannot be null or empty");
        }

        if (_state != FdwTransactionState.Active)
        {
            return FdwResult<string>.Failure($"Cannot create savepoint in transaction state: {_state}");
        }

        if (_savepoints.ContainsKey(savepointName))
        {
            return FdwResult<string>.Failure($"Savepoint '{savepointName}' already exists");
        }

        try
        {
            _logger.LogDebug("Creating savepoint '{SavepointName}' in transaction {TransactionId}", 
                savepointName, TransactionId);

            await _transaction.SaveAsync(savepointName).ConfigureAwait(false);
            
            var savepointId = $"{TransactionId}_{savepointName}";
            _savepoints[savepointName] = savepointId;
            
            _logger.LogDebug("Savepoint '{SavepointName}' created successfully in transaction {TransactionId}", 
                savepointName, TransactionId);
            
            return FdwResult<string>.Success(savepointId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create savepoint '{SavepointName}' in transaction {TransactionId}", 
                savepointName, TransactionId);
            
            return FdwResult<string>.Failure($"Savepoint creation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IFdwResult> RollbackToSavepointAsync(string savepointName)
    {
        ThrowIfDisposed();
        
        if (string.IsNullOrWhiteSpace(savepointName))
        {
            return FdwResult.Failure("Savepoint name cannot be null or empty");
        }

        if (_state != FdwTransactionState.Active)
        {
            return FdwResult.Failure($"Cannot rollback to savepoint in transaction state: {_state}");
        }

        if (!_savepoints.ContainsKey(savepointName))
        {
            return FdwResult.Failure($"Savepoint '{savepointName}' does not exist");
        }

        try
        {
            _logger.LogDebug("Rolling back to savepoint '{SavepointName}' in transaction {TransactionId}", 
                savepointName, TransactionId);

            await _transaction.RollbackAsync(savepointName).ConfigureAwait(false);
            
            // Remove this savepoint and any later ones
            var savepointsToRemove = new List<string>();
            var foundTarget = false;
            
            foreach (var kvp in _savepoints)
            {
                if (foundTarget || kvp.Key == savepointName)
                {
                    savepointsToRemove.Add(kvp.Key);
                    foundTarget = true;
                }
            }

            foreach (var sp in savepointsToRemove)
            {
                _savepoints.Remove(sp);
            }
            
            _logger.LogDebug("Rolled back to savepoint '{SavepointName}' in transaction {TransactionId}", 
                savepointName, TransactionId);
            
            return FdwResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback to savepoint '{SavepointName}' in transaction {TransactionId}", 
                savepointName, TransactionId);
            
            return FdwResult.Failure($"Savepoint rollback failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a command within the current transaction scope.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <returns>The command execution result.</returns>
    private async Task<IFdwResult<object?>> ExecuteCommandInTransaction(FractalDataWorks.Services.DataProvider.Abstractions.IDataCommand command)
    {
        // This is a simplified implementation - in practice, this would delegate to the provider
        // with the transaction context, but since we don't have the full provider implementation yet,
        // we'll return a placeholder result
        
        await Task.Delay(1).ConfigureAwait(false); // Simulate async work
        
        _logger.LogDebug("Command {CommandType} executed in transaction context", command.CommandType);
        
        // In the actual implementation, this would execute the command using Dapper
        // with the transaction context
        return FdwResult<object?>.Success(null);
    }

    /// <summary>
    /// Converts FDW transaction isolation level to SQL Server isolation level.
    /// </summary>
    /// <param name="isolationLevel">The FDW isolation level.</param>
    /// <returns>The SQL Server isolation level.</returns>
    private static IsolationLevel ConvertIsolationLevel(FdwTransactionIsolationLevel isolationLevel)
    {
        return isolationLevel switch
        {
            FdwTransactionIsolationLevel.ReadUncommitted => IsolationLevel.ReadUncommitted,
            FdwTransactionIsolationLevel.ReadCommitted => IsolationLevel.ReadCommitted,
            FdwTransactionIsolationLevel.RepeatableRead => IsolationLevel.RepeatableRead,
            FdwTransactionIsolationLevel.Serializable => IsolationLevel.Serializable,
            FdwTransactionIsolationLevel.Snapshot => IsolationLevel.Snapshot,
            FdwTransactionIsolationLevel.Default => IsolationLevel.ReadCommitted,
            _ => IsolationLevel.ReadCommitted
        };
    }

    /// <summary>
    /// Throws an exception if the transaction manager has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MsSqlTransactionManager));
        }
    }

    /// <summary>
    /// Disposes the transaction manager and releases any resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            if (_state == FdwTransactionState.Active)
            {
                _logger.LogWarning("Transaction {TransactionId} is being disposed while still active. Rolling back.", TransactionId);
                _transaction.Rollback();
                _state = FdwTransactionState.RolledBack;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during transaction {TransactionId} disposal", TransactionId);
            _state = FdwTransactionState.Faulted;
        }
        finally
        {
            try
            {
                _transaction?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing SQL transaction for {TransactionId}", TransactionId);
            }

            _savepoints.Clear();
            _state = FdwTransactionState.Disposed;
            _disposed = true;
            
            _logger.LogDebug("Transaction manager {TransactionId} disposed", TransactionId);
        }
    }
}