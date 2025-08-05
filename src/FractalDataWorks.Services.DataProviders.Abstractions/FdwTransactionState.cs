namespace FractalDataWorks.Services.DataProviders.Abstractions;

/// <summary>
/// Defines the possible states of a data transaction in the FractalDataWorks framework.
/// </summary>
/// <remarks>
/// Transaction states help track the lifecycle of data transactions and ensure
/// proper transaction management, error handling, and resource cleanup.
/// </remarks>
public enum FdwTransactionState
{
    /// <summary>
    /// The transaction is in an unknown or uninitialized state.
    /// </summary>
    Unknown = 0,
    
    /// <summary>
    /// The transaction has been created but not yet started.
    /// </summary>
    Created = 1,
    
    /// <summary>
    /// The transaction is active and can execute commands.
    /// </summary>
    Active = 2,
    
    /// <summary>
    /// The transaction is currently executing a command.
    /// </summary>
    Executing = 3,
    
    /// <summary>
    /// The transaction is in the process of being committed.
    /// </summary>
    Committing = 4,
    
    /// <summary>
    /// The transaction has been successfully committed.
    /// </summary>
    Committed = 5,
    
    /// <summary>
    /// The transaction is in the process of being rolled back.
    /// </summary>
    RollingBack = 6,
    
    /// <summary>
    /// The transaction has been rolled back.
    /// </summary>
    RolledBack = 7,
    
    /// <summary>
    /// The transaction is in a faulted state due to an error.
    /// </summary>
    Faulted = 8,
    
    /// <summary>
    /// The transaction has been disposed and cannot be reused.
    /// </summary>
    Disposed = 9
}

/// <summary>
/// Defines the isolation levels available for data transactions in the FractalDataWorks framework.
/// </summary>
/// <remarks>
/// Isolation levels control the degree to which transactions are isolated from each other
/// and determine what data changes are visible between concurrent transactions.
/// Not all providers support all isolation levels.
/// </remarks>
public enum FdwTransactionIsolationLevel
{
    /// <summary>
    /// The default isolation level for the provider.
    /// </summary>
    Default = 0,
    
    /// <summary>
    /// No isolation is provided. Transactions may see uncommitted changes from other transactions.
    /// </summary>
    /// <remarks>
    /// This is the lowest isolation level and provides the best performance but the least
    /// consistency guarantees. Use with caution and only when data consistency is not critical.
    /// </remarks>
    ReadUncommitted = 1,
    
    /// <summary>
    /// Transactions can only see committed changes from other transactions.
    /// </summary>
    /// <remarks>
    /// This isolation level prevents dirty reads but allows non-repeatable reads and phantom reads.
    /// It provides a balance between performance and consistency.
    /// </remarks>
    ReadCommitted = 2,
    
    /// <summary>
    /// Transactions see a consistent snapshot of data throughout their execution.
    /// </summary>
    /// <remarks>
    /// This isolation level prevents dirty reads and non-repeatable reads but may allow phantom reads.
    /// It provides good consistency with reasonable performance characteristics.
    /// </remarks>
    RepeatableRead = 3,
    
    /// <summary>
    /// Transactions are completely isolated from each other.
    /// </summary>
    /// <remarks>
    /// This is the highest isolation level and prevents all read phenomena (dirty reads,
    /// non-repeatable reads, and phantom reads). It provides the strongest consistency
    /// guarantees but may impact performance due to increased locking.
    /// </remarks>
    Serializable = 4,
    
    /// <summary>
    /// Transactions see a consistent snapshot of the database at the time they start.
    /// </summary>
    /// <remarks>
    /// This isolation level provides snapshot isolation, which prevents most read phenomena
    /// while allowing better concurrency than serializable isolation. Not all providers
    /// support this isolation level.
    /// </remarks>
    Snapshot = 5
}