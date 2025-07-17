namespace FractalDataWorks.Data;

/// <summary>
/// Defines the types of data operations.
/// </summary>
public enum DataOperation
{
    /// <summary>
    /// Query operation.
    /// </summary>
    Query,

    /// <summary>
    /// Insert operation.
    /// </summary>
    Insert,

    /// <summary>
    /// Update operation.
    /// </summary>
    Update,

    /// <summary>
    /// Upsert operation.
    /// </summary>
    Upsert,

    /// <summary>
    /// Delete operation.
    /// </summary>
    Delete,

    /// <summary>
    /// Bulk insert operation.
    /// </summary>
    BulkInsert,

    /// <summary>
    /// Bulk update operation.
    /// </summary>
    BulkUpdate,

    /// <summary>
    /// Bulk delete operation.
    /// </summary>
    BulkDelete
}