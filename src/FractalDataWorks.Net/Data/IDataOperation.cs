namespace FractalDataWorks.Data;

/// <summary>
/// Base interface for all data operations.
/// Avoids naming collision with System.Windows.Input.ICommand.
/// </summary>
public interface IDataOperation
{
    /// <summary>
    /// Unique identifier for this operation instance
    /// </summary>
    string OperationId { get; }
    
    /// <summary>
    /// The type of operation being performed
    /// </summary>
    OperationType OperationType { get; }
    
    /// <summary>
    /// Additional context data for the operation
    /// </summary>
    Dictionary<string, object> Context { get; }
    
    /// <summary>
    /// When the operation was created
    /// </summary>
    DateTime CreatedAt { get; }
}

/// <summary>
/// Types of data operations
/// </summary>
public enum OperationType
{
    /// <summary>
    /// Query/read operation
    /// </summary>
    Query,
    
    /// <summary>
    /// Insert/create operation
    /// </summary>
    Insert,
    
    /// <summary>
    /// Update/modify operation
    /// </summary>
    Update,
    
    /// <summary>
    /// Delete/remove operation
    /// </summary>
    Delete,
    
    /// <summary>
    /// Upsert (insert or update) operation
    /// </summary>
    Upsert,
    
    /// <summary>
    /// Find by identity operation
    /// </summary>
    Find
}