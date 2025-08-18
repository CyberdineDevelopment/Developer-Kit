namespace FractalDataWorks.Services.DataProvider.Abstractions;

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