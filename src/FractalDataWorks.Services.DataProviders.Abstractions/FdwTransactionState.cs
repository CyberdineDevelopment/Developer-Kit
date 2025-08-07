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