namespace FractalDataWorks.Data;

/// <summary>
/// Represents an insert operation - a type of data operation
/// </summary>
/// <typeparam name="T">The entity type being inserted</typeparam>
public interface IDataInsert<T> : IDataOperation where T : class
{
    /// <summary>
    /// The entity to insert
    /// </summary>
    T Entity { get; }
    
    /// <summary>
    /// Whether to return generated keys (like auto-increment IDs)
    /// </summary>
    bool ReturnGeneratedKeys { get; }
    
    /// <summary>
    /// The target container name (table, file, etc.)
    /// </summary>
    string TargetName { get; }
}