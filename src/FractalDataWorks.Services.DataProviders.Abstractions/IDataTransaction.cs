using System;
using System.Threading.Tasks;


namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Interface for data transactions in the FractalDataWorks framework.
/// Provides atomic execution capabilities for multiple data commands.
/// </summary>
/// <remarks>
/// Data transactions ensure that multiple data operations either all succeed or all fail,
/// maintaining data consistency across complex operations. Transactions provide isolation
/// and enable rollback of partially completed operations.
/// </remarks>
public interface IDataTransaction : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for this transaction.
    /// </summary>
    /// <value>A unique identifier for the transaction instance.</value>
    /// <remarks>
    /// Transaction identifiers are used for tracking, logging, and debugging purposes.
    /// They help correlate transaction operations across distributed systems.
    /// </remarks>
    string TransactionId { get; }
    
    /// <summary>
    /// Gets the current state of the transaction.
    /// </summary>
    /// <value>The current transaction state.</value>
    /// <remarks>
    /// Transaction state helps determine what operations are valid and whether
    /// the transaction can be committed or must be rolled back.
    /// </remarks>
    FdwTransactionState State { get; }
    
    /// <summary>
    /// Gets the isolation level for this transaction.
    /// </summary>
    /// <value>The transaction isolation level.</value>
    /// <remarks>
    /// Isolation levels control how changes made by one transaction are visible
    /// to other concurrent transactions. Different providers may support different
    /// isolation levels based on their underlying technology.
    /// </remarks>
    FdwTransactionIsolationLevel IsolationLevel { get; }
    
    /// <summary>
    /// Gets the timeout for this transaction.
    /// </summary>
    /// <value>The maximum time the transaction can remain active, or null for no timeout.</value>
    /// <remarks>
    /// Transaction timeouts prevent long-running transactions from holding locks
    /// indefinitely and affecting system performance. When exceeded, transactions
    /// are automatically rolled back.
    /// </remarks>
    TimeSpan? Timeout { get; }
    
    /// <summary>
    /// Gets the timestamp when this transaction was started.
    /// </summary>
    /// <value>The UTC timestamp when the transaction began.</value>
    /// <remarks>
    /// Start time helps calculate transaction duration and identify long-running
    /// transactions that may need attention or optimization.
    /// </remarks>
    DateTimeOffset StartedAt { get; }
    
    /// <summary>
    /// Gets the data provider that owns this transaction.
    /// </summary>
    /// <value>The data provider instance that created this transaction.</value>
    /// <remarks>
    /// The owning provider handles transaction execution and ensures proper
    /// coordination with the underlying data store technology.
    /// </remarks>
    IDataService Provider { get; }
    
    /// <summary>
    /// Executes a data command within this transaction scope.
    /// </summary>
    /// <param name="command">The data command to execute.</param>
    /// <returns>
    /// A task representing the asynchronous command execution operation.
    /// The result contains the command execution outcome and any returned data.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction is not in a valid state for command execution.
    /// </exception>
    /// <remarks>
    /// Commands executed within a transaction scope participate in the transaction's
    /// atomicity guarantees. They will be committed or rolled back together with
    /// all other commands in the transaction.
    /// </remarks>
    Task<IFdwResult<object?>> Execute(IDataCommand command);
    
    /// <summary>
    /// Executes a typed data command within this transaction scope.
    /// </summary>
    /// <typeparam name="TResult">The expected type of the command result.</typeparam>
    /// <param name="command">The typed data command to execute.</param>
    /// <returns>
    /// A task representing the asynchronous command execution operation.
    /// The result contains the strongly-typed command execution outcome.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction is not in a valid state for command execution.
    /// </exception>
    /// <remarks>
    /// This method provides type-safe command execution within the transaction scope,
    /// eliminating the need for runtime type checking and casting.
    /// </remarks>
    Task<IFdwResult<TResult>> Execute<TResult>(IDataCommand<TResult> command);
    
    /// <summary>
    /// Commits all changes made within this transaction.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous commit operation.
    /// The result indicates whether the commit was successful.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction is not in a valid state for committing.
    /// </exception>
    /// <remarks>
    /// Committing a transaction makes all changes permanent and releases any locks
    /// held by the transaction. After committing, the transaction cannot be used
    /// for further operations.
    /// </remarks>
    Task<IFdwResult> CommitAsync();
    
    /// <summary>
    /// Rolls back all changes made within this transaction.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous rollback operation.
    /// The result indicates whether the rollback was successful.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction is not in a valid state for rollback.
    /// </exception>
    /// <remarks>
    /// Rolling back a transaction undoes all changes made within the transaction
    /// and releases any locks held. After rollback, the transaction cannot be used
    /// for further operations. This method is idempotent and safe to call multiple times.
    /// </remarks>
    Task<IFdwResult> RollbackAsync();
    
    /// <summary>
    /// Creates a savepoint within this transaction.
    /// </summary>
    /// <param name="savepointName">The name for the savepoint.</param>
    /// <returns>
    /// A task representing the asynchronous savepoint creation operation.
    /// The result contains the savepoint identifier if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="savepointName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="savepointName"/> is empty or whitespace.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transaction does not support savepoints or is not in a valid state.
    /// </exception>
    /// <remarks>
    /// Savepoints enable partial rollback within a transaction, allowing recovery
    /// from errors without losing all transaction work. Not all providers support savepoints.
    /// </remarks>
    Task<IFdwResult<string>> CreateSavepointAsync(string savepointName);
    
    /// <summary>
    /// Rolls back to a previously created savepoint.
    /// </summary>
    /// <param name="savepointName">The name of the savepoint to roll back to.</param>
    /// <returns>
    /// A task representing the asynchronous savepoint rollback operation.
    /// The result indicates whether the rollback was successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="savepointName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="savepointName"/> is empty or whitespace.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the savepoint does not exist or the transaction is not in a valid state.
    /// </exception>
    /// <remarks>
    /// Rolling back to a savepoint undoes all changes made after the savepoint was created
    /// while preserving changes made before the savepoint. The transaction remains active.
    /// </remarks>
    Task<IFdwResult> RollbackToSavepointAsync(string savepointName);
}